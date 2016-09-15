namespace ModMyFactory.Win32
{
    enum WindowLongIndex
    {
        /// <summary>
        /// Retrieves the extended window styles.
        /// </summary>
        ExtendedStyle = -20,

        /// <summary>
        /// Retrieves a handle to the application instance.
        /// </summary>
        ApplicationHandle = -6,

        /// <summary>
        /// Retrieves a handle to the parent window, if any.
        /// </summary>
        ParentHandle = -8,

        /// <summary>
        /// Retrieves the identifier of the window.
        /// </summary>
        Id = -12,

        /// <summary>
        /// Retrieves the window styles.
        /// </summary>
        Style = -16,

        /// <summary>
        /// Retrieves the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero.
        /// </summary>
        Userdata = -21,

        /// <summary>
        /// Retrieves the address of the window procedure, or a handle representing the address of the window procedure. You must use the CallWindowProc function to call the window procedure.
        /// </summary>
        WndProc = -4,
    }
}
