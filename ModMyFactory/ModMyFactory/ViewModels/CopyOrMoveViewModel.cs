using System.ComponentModel;
using ModMyFactory.Models;
using WPFCore;

namespace ModMyFactory.ViewModels
{
    class CopyOrMoveViewModel : ViewModelBase
    {
        CopyOrMoveType copyOrMoveType;

        public CopyOrMoveType CopyOrMoveType
        {
            get { return copyOrMoveType; }
            set
            {
                if (value != copyOrMoveType)
                {
                    copyOrMoveType = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(CopyOrMoveType)));
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(Message)));
                }
            }
        }

        public string Message
        {
            get
            {
                switch (CopyOrMoveType)
                {
                    case CopyOrMoveType.Factorio:
                        return App.Instance.GetLocalizedResourceString("CopyOrMoveFactorioMessage");
                    case CopyOrMoveType.Mods:
                        return App.Instance.GetLocalizedResourceString("CopyOrMoveModsMessage");
                    case CopyOrMoveType.Mod:
                        return App.Instance.GetLocalizedResourceString("CopyOrMoveModMessage");
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
