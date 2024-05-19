namespace Pixeval.CoreApi;

internal interface IDefaultFactory<out TSelf> where TSelf : IDefaultFactory<TSelf>
{
    static abstract TSelf CreateDefault();
}

public static class DefaultImageUrls
{
    public const string Prefix = "ms-appx:///Assets/Images/";
    public const string ImageNotAvailable = Prefix + "image-not-available.png";
    public const string NoProfile = Prefix + "pixiv_no_profile.png";
}
