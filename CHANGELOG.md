#### 1.10.0
* Added support for multiple versions of the same mod.
* Added option to lock modpacks.
* Added support for mod thumbnails.
* Added changelog to About section.
* Fixed crash when selecting certain online mods.
* Fixed rare crash when adding Steam installation.
* Fixed visual bug in expanded list items.
* Improved filtering performance.

#### 1.9.0
* Added proper handling for inverted mod dependencies.
* Added theme support (light and dark theme available now).
* Removed autosaves from the save selection when creating a link.
* Fixed crash when adding Factorio from folder.
* Fixed an issue that caused some mods to not be shown in the online list.
* Fixed bug that caused deactivated releases in the online mods window to be selected.

#### 1.8.6
* Improved the update notification to include the changelog.
* Optimized loading of the online mod list.
* Installed deprecated mods are now shown in the online list.

#### 1.8.5
* Improved the behaviour when importing modpacks: mods that are already installed will no longer be downloaded and will also not cause warning messages.
* Improved behaviour of dragging and dropping mods from Windows Explorer to ModMyFactory: the Explorer will no longer be blocked until the operation is complete and mods are now copied, not moved.
* Fixed an issue that would cause the names of Factorio installations to get messed up when refreshing.
* Fixed a bug in command line parsing that would sometimes cause ModMyFactory to not import specified modpack files.
* Fixed an issue that caused ModMyFactory to not import modpack files from Windows Explorer if it was already running.
* Fixed a bug that caused the update check on startup to not work even when enabled in the settings.

#### 1.8.4
* Fixed an issue where having a mod installed that is not available on the mod portal was preventing other mods from being updated.

#### 1.8.3
* Added a dependency list for each mod.

#### 1.8.2
* Added context menu options to activate mod dependencies.
* Added drag and drop support for Factorio ZIP archives in the Version Manager.
* Fixed a bug where deleted Factorio versions would still count towards the unique names.
* Removed an obsolete warning message on link creation.
* Fixed --factorio-version command line argument not working.

#### 1.8.1
* Added some missing progress windows and error messages in the Version Manager.
* Fixed that downloading Factorio would show incorrect versions to select.
* Fixed that ModMyFactory would allow non-unique Factorio names in certain situations.
* Fixed that links targeting the "Latest installed version" would use an incorrect version name.

#### 1.8.0
* Improved how Factorio versions are managed: there can now be multiple installations of the same version, and every installation can be given a custom name.
* Links have been changed to make use of the new naming system. Old links may now be non-functional, however links created from now on will not be broken by updates anymore (they do break when you change the name of the corresponding Factorio installation).
* The Steam version of Factorio can now be added automatically with no need to select the installation directory anymore. If you had the Steam version added previously you will be required to re-add it once after updating.
* Added more context menu options.
* Added dependency warnings for mods inside modpacks.
* Fixed downloading of Factorio, which had been broken due to changes to the website.
* Fixed a crash that was caused by trying to log in with an account that doesn't own a Factorio license.
* Fixed a problem with certain mod dependencies.
* Fixed a crash related to improper loading of some mods.

#### 1.7.2
* Hotfix for a hard crash when the mod directory was not present on start.

#### 1.7.1
* Added drag and drop support for mod files and folders.
* Added an option to filter mods on the mod portal by Factorio version.
* Fixed a problem with importing mod folders.
* Fixed a possible crash related to mod importing.
* Various other small improvements and fixes.

#### 1.7.0
* Added mod dependency support.

#### 1.6.0
* Complete rework of modpack sharing.
* Added more options to links.
* Made modpack names unique.

#### 1.5.12
* Restored functionality after the mod portal update.
* Added Korean translation.
* Added "New Modpack" entry to modpack list context menu.
* Dragging and dropping mods into the modpack list at a free space will now create a new modpack containing all the dropped mods.

#### 1.5.11
* Allowed to update Factorio even when the target update was already present through Steam.
* Added Chinese translation.
* Improved layout of mod update window.

#### 1.5.10
* Fixed an issue introduced in 1.5.9 that prevented windows from remembering their size.
* Improved sorting in the Factorio download list.
* A Factorio version that is already present through Steam can now also be downloaded separately.

#### 1.5.9
* Fixed a rare hang condition in the online mod window.
* Fixed that windows could appear on disconnected displays.

#### 1.5.8
* Fixed mods appearing multiple times in some situations after updating them.

#### 1.5.6
* Added settings to more precisely control which mods to keep when updating mods.
* Mods will now always be updated to their newest version regardless of the manager mode (mods for old versions are kept by default).
* Added a Factorio version indicator to mods inside modpacks.
* Added the possibility to create links that only target a main version of Factorio instead of a specific version.
* Fixed that multiple version of the same mod where loaded in the wrong order (highest version now first).
* Fixed that the links in the 'View' menu would always take you to the appdata folder regardless of your setting.

#### 1.5.4
* Adjusted settings default values.
* Fixed crash when connection probelm occurs while fetching mods.
* Updated translator list.

#### 1.5.3
* Added list support in formatted mod description.
* The newest available release of a mod is now auto-selected when selecting the mod.
* Fixed a bug that caused links in the formatted mod description to be parsed incorrectly in rare cases.

#### 1.5.2
* Fixed crash when selecting an online mod.
* Login now works again after change to factorio.com website.

#### 1.5.1
* Rearranged some menu items.
* Added option to manually refresh mod list.
* Added some basic formatting support to the mod description of mods.factorio.com.

#### 1.5.0
* Locations of savegames and scenarios can now be changed in the settings.
* Multiple versions of a mod for the same version of Factorio being present is now handled properly (newest version will always be used just like in Factorio).
* Added context menus to the mod and modpack listview.
* ModMyFactory now displays some more information about mods from the modportal (license, homepage, GitHub).
* Added option to show/hide experimental versions when downloading Factorio.
* Added option to disable the automatic update check in the settings.
* Added option to copy instead of move when adding Factorio from folder.
* Added option to opt in for prerelease updates.
* Added option to always keep mod updates zipped.
* Added option to keep old mod versions after updating.
* Fixed bug that would prevent adding Factorio from ZIP (would always show an error message).
* Added scrollbar to mod update list.
* Made info.json parsing less strict (Tomb Stones mod will now work).
* Mods are now properly reloaded when the game is started through a link that alters the mods active states.
* Added per monitor DPI scaling support (requires .Net 4.6.2 and Windows 10 Anniversary).

#### 1.4.3
* Added the option to delete mods from the online view.
* Allowed links to work while ModMyFactory is running.
* Added check if version is already installed when adding Factorio from ZIP and folder.
* Improved mod loading logic (invalid archives and folders are now ignored instead of crashing ModMyFactory).
* Improved performance when activating/deactivating all mods at once.
* Fixed crash when trying to log in with incorrect credentials.
* Fixed crash when trying to add older versions of Factorio (0.10 and older) from ZIP.
* Fixed that the releases list of mods on the mod portal was not updated properly in some cases.

#### 1.4.2
* Fixed crash when adding a Factorio version from folder that contained an unzipped mod.
* Improved behaviour when moving Factorio and mod location in the settings (will now only move Factorio/mods and leave other contents).
* Added Portuguese translation.

#### 1.4.1
* Fixed links not properly being recreated after updating Factorio.
* Fixed selected version not being saved in some cases after it was updated.

#### 1.4.0
* Factorio (non-Steam) can now be updated in the version manager.
* Added Russian translation.
* Improved modpack GUI to highlight subitems.
* Optimized filter algorithm.

#### 1.3.3
* ModMyFactory will no longer start cmd processes.
* Fixed error message falsely being displayed when mod is not available on mod portal.
* Fixed possible crash when displaying the crash message.
* Further improved the filter algorithm.
* Fixed possible crash when moving folders.

#### 1.3.2
* Improved filter algorithm.

#### 1.3.1
* Improved high DPI scaling in .Net 4.6 and above.
* Fixed toolbar buttons not being square.
* Added link to wiki.

#### 1.3.0
* The mod list fetched from mods.factorio.com is now cached and not redownloaded every time the list is opened.
* ModMyFactory now offers functionality to store login credentials.
* All mods/modpacks can now be activated/deactivated at once using the checkboxes at the top.
* Network errors are now be handled properly.

#### 1.2.2
* Fixed possible crash when searching for mod updates.
* Fixed misbehavior when changing the mod folder.

#### 1.2.1
* Fixed possible crash when opening a folder.
* Fixed crash when adding Factorio from folder (also applies to Steam version).
* Fixed possible crash when changing a folder in the settings.

#### 1.2.0
* The Steam version of Factorio is now supported.
* Modpacks can now be exported and imported.
* Fixed misbehaviour with mods containing '_' in their name.
* Fixed crash when cancelling a mod download.
* Fixed possible crash when adding Factorio from folder (also applies to Steam version).

#### 1.1.4
* Fixed possible crash when changing the Factorio folder in the settings.

#### 1.1.3
* ModMyFactory now also saves position and size of the mod manager and mod downloading windows.
* Unchecking a mod in the update dialog will now actually not update that mod.
* When updating mod, the modpacks containing the updated mod are now properly saved.
* The update notification now redirects to a proper browser-viewable site.

#### 1.1.1
* Fixed crash when clicking in the mod managers list view.
* Item controls no longer auto-select items.
* Modpacks are now sorted alphabetically in the link properties.
* Fixed a possible bug regarding the asynchronous loading of online mod information.

#### 1.1.0
* Added mod downloading and updating.
* Two different manager modes are now available.
* 'Latest' is now a valid Factorio version in links and will start the latest available Factorio version.
* ModMyFactory will now create crash logs in %AppData%\ModMyFactory.

#### 1.0.6
* ModMyFactory will now inform you about new updates (can also be checked manually).
* Factorio versions added from folder will now always show without a restart.

#### 1.0.5
* Fixed a critical bug that caused the application to crash when clicking in the list views without selecting an item.

#### 1.0.4
* Mods and modpacks can now be filtered.
* New button in version manager to open the Factorio folder.
* Forum thread is now linked in the 'Info' menu.
* Renaming modpacks does now respon to the enter key and to lost focus.
* All hotkeys containing the Shift key have been changed to avoid confilcts with writing uppercase letters.

#### 1.0.2
* First public release.