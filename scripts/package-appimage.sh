#!/usr/bin/env bash
set -euo pipefail

usage() {
	printf 'Usage: %s <publish-dir> <output-dir> [version]\n' "$0" >&2
}

if [[ $# -lt 2 || $# -gt 3 ]]; then
	usage
	exit 2
fi

publish_dir=$(realpath "$1")
output_dir=$(realpath -m "$2")
version=${3:-0.0.0}
repo_root=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
app_name=Pixeval
app_id=io.github.Pixeval.Pixeval
app_dir=$(mktemp -d)
appimage_tool=${APPIMAGETOOL:-}

case $(uname -m) in
	x86_64 | amd64)
		appimage_arch=x86_64
		;;
	aarch64 | arm64)
		appimage_arch=aarch64
		;;
	*)
		printf 'Unsupported AppImage architecture: %s\n' "$(uname -m)" >&2
		exit 1
		;;
esac

cleanup() {
	rm -rf "$app_dir"
}
trap cleanup EXIT

if [[ ! -d "$publish_dir" ]]; then
	printf 'Publish directory does not exist: %s\n' "$publish_dir" >&2
	exit 1
fi

if [[ ! -f "$publish_dir/Pixeval.Desktop" ]]; then
	printf 'Published executable was not found: %s\n' "$publish_dir/Pixeval.Desktop" >&2
	exit 1
fi

mkdir -p \
	"$app_dir/usr/bin" \
	"$app_dir/usr/share/applications" \
	"$app_dir/usr/share/icons/hicolor/scalable/apps" \
	"$app_dir/usr/share/metainfo" \
	"$output_dir"

tar \
	--exclude='*.pdb' \
	--exclude='*.dbg' \
	-C "$publish_dir" \
	-cf - . | tar -C "$app_dir/usr/bin" -xf -
chmod +x "$app_dir/usr/bin/Pixeval.Desktop"

desktop_file="$app_dir/$app_id.desktop"
cat > "$desktop_file" <<EOF
[Desktop Entry]
Type=Application
Name=$app_name
Comment=Wow. Yet another Pixiv client!
Exec=Pixeval.Desktop
Icon=$app_id
Terminal=false
Categories=Graphics;
StartupWMClass=Pixeval
EOF

cp "$desktop_file" "$app_dir/usr/share/applications/$app_id.desktop"
cat > "$app_dir/usr/share/metainfo/$app_id.appdata.xml" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<component type="desktop-application">
  <id>$app_id</id>
  <metadata_license>CC0-1.0</metadata_license>
  <project_license>GPL-3.0-only</project_license>
  <name>$app_name</name>
  <summary>Wow. Yet another Pixiv client!</summary>
  <description>
    <p>基于.NET 10 和 Avalonia 的强大、快速、漂亮的Pixiv第三方应用程序</p>
  </description>
  <developer id="io.github.pixeval">
    <name>Pixeval contributors</name>
  </developer>
  <url type="homepage">https://github.com/Pixeval/Pixeval</url>
  <content_rating type="oars-1.1" />
  <launchable type="desktop-id">$app_id.desktop</launchable>
</component>
EOF
cp "$repo_root/src/Pixeval/Assets/logo.svg" "$app_dir/$app_id.svg"
cp "$repo_root/src/Pixeval/Assets/logo.svg" "$app_dir/.DirIcon"
cp "$repo_root/src/Pixeval/Assets/logo.svg" "$app_dir/usr/share/icons/hicolor/scalable/apps/$app_id.svg"

cat > "$app_dir/AppRun" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail

here=$(dirname "$(readlink -f "${BASH_SOURCE[0]}")")
exec "$here/usr/bin/Pixeval.Desktop" "$@"
EOF
chmod +x "$app_dir/AppRun"

if [[ -z "$appimage_tool" ]]; then
	tool_cache_dir=${APPIMAGETOOL_CACHE_DIR:-}
	if [[ -z "$tool_cache_dir" ]]; then
		if [[ -n "${RUNNER_TEMP:-}" ]]; then
			tool_cache_dir="$RUNNER_TEMP/appimage-tools"
		else
			tool_cache_dir="${XDG_CACHE_HOME:-${HOME:-$output_dir/.cache}}/appimage-tools"
		fi
	fi

	mkdir -p "$tool_cache_dir"
	appimage_tool="$tool_cache_dir/appimagetool-$appimage_arch.AppImage"
	if [[ ! -f "$appimage_tool" ]]; then
		curl -fsSL \
			-o "$appimage_tool" \
			"https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-$appimage_arch.AppImage"
		chmod +x "$appimage_tool"
	fi
fi

appimage_name="$app_name-$version-linux-$appimage_arch.AppImage"
ARCH=$appimage_arch VERSION="$version" APPIMAGE_EXTRACT_AND_RUN=1 "$appimage_tool" "$app_dir" "$output_dir/$appimage_name"
chmod +x "$output_dir/$appimage_name"
printf '%s\n' "$output_dir/$appimage_name"
