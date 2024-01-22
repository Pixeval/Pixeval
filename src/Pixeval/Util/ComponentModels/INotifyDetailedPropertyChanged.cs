namespace Pixeval.Util.ComponentModels;

public interface INotifyDetailedPropertyChanged
{
    event DetailedPropertyChangedEventHandler? DetailedPropertyChanged;
}
