using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Pixeval.Controls.IllustratorView;

namespace Pixeval.Pages.IllustratorViewer;

public sealed partial class IllustratorPageViewModel : ObservableObject
{
    [ObservableProperty]
    private IllustratorPageIllustratorModel? _illustratorViewModel;


}