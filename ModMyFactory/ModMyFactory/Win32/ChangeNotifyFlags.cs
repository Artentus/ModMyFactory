using System;

namespace ModMyFactory.Win32
{
    [Flags]
    public enum ChangeNotifyFlags
    {
        DWord = 0x0003,
        IdList = 0x0000,
        PathA = 0x0001,
        PathW = 0x0005,
        PrinterA = 0x0002,
        PrinterW = 0x0006,
        Flush = 0x1000,
        FlushNoWait = 0x2000
    }
}
