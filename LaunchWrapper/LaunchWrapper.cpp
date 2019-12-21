// Pixeval - A Strong, Fast and Flexible Pixiv Client
// Copyright (C) 2019 Dylech30th
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#include <iostream>
#include <vector>
#include <filesystem>
#include <regex>
#include <Windows.h>
#include <processenv.h>

bool netcore_3_being_fully_installed();
bool netcore_app_installed();
bool windows_desktop_installed();
bool directory_name_match_netcore_3(const std::string&);
std::string program_files_folder();
std::vector<std::string> get_directories_name(const std::string&);

int main() noexcept  // NOLINT(bugprone-exception-escape)
{
	ShowWindow(GetConsoleWindow(), SW_HIDE);
	if (!netcore_3_being_fully_installed())
	{
		ShowWindow(GetConsoleWindow(), SW_SHOW);
		std::cout << "Pixeval cannot detect .NET Core Runtime, Installing for you..." << std::endl;
		system("dotnet-install.bat");
		std::cout << "Install finished" << std::endl;
	}
	system("pInternal.exe");
}

bool netcore_3_being_fully_installed()
{
	return netcore_app_installed() && windows_desktop_installed();
}

bool netcore_app_installed()
{
	auto vec = get_directories_name(program_files_folder() + R"(\dotnet\shared\Microsoft.NETCore.App)");

	for (const auto& name : vec)
	{
		if (directory_name_match_netcore_3(name))
			return true;
	}
	return false;
}

bool windows_desktop_installed()
{
	auto vec = get_directories_name(program_files_folder() + R"(\dotnet\shared\Microsoft.WindowsDesktop.App)");

	for (const auto& name : vec)
	{
		if (directory_name_match_netcore_3(name))
			return true;
	}
	return false;
}

bool directory_name_match_netcore_3(const std::string& name)
{
	return std::regex_match(name, std::regex(R"(3.\d+.\d+)"));
}

std::string program_files_folder()
{
	WCHAR folder[100];
	ExpandEnvironmentStrings(L"%ProgramW6432%", folder, ARRAYSIZE(folder));

	std::wstring ws(folder);
	return std::string(ws.begin(), ws.end());
}

std::vector<std::string> get_directories_name(const std::string& path)
{
	std::vector<std::string> directory_name;
	const auto itr = std::filesystem::directory_iterator(path);
	std::for_each(begin(itr), end(itr), [&directory_name](const std::filesystem::directory_entry& directory_entry)
		{
			if (directory_entry.is_directory())
				directory_name.push_back(directory_entry.path().filename().string());
		});
	return directory_name;
}
