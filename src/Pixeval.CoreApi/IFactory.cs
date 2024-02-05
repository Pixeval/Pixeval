namespace Pixeval.CoreApi;

public interface IFactory<out T> where T : IFactory<T>
{
    static abstract T CreateDefault();
}

public static class DefaultImageUrls
{
    public const string Prefix = "ms-appx:///Assests/Images/";
    public const string ImageNotAvailable = Prefix + "image-not-available.png";
    public const string NoProfile = Prefix + "pixiv_no_profile.png";
}
