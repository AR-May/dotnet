// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Description: The ApplicationCommands class defines a standard set of commands that are required in most applications.
//              The goal of these commands is to unify input, programming model and UI for the most common actions in
//              Windows applications thus providing a standard interface for such common commands.
//
//              See spec at : http://avalon/CoreUI/Specs%20%20Eventing%20and%20Commanding/CommandLibrarySpec.mht

using MS.Internal;

namespace System.Windows.Input
{
    /// <summary>
    /// ApplicationCommands - Set of Standard Commands
    /// </summary>
    public static class ApplicationCommands
    {
        //------------------------------------------------------
        //
        //  Public Methods
        //
        //------------------------------------------------------
#region Public Methods
        /// <summary>
        /// CutCommand - action to cut selection
        /// </summary>
        public static RoutedUICommand Cut
        {
            get { return _EnsureCommand(CommandId.Cut); }
        }

        /// <summary>
        /// CopyCommand
        /// </summary>
        public static RoutedUICommand Copy
        {
            get { return _EnsureCommand(CommandId.Copy); }
        }

        /// <summary>
        /// PasteCommand
        /// </summary>
        public static RoutedUICommand Paste
        {
            get { return _EnsureCommand(CommandId.Paste); }
        }

        /// <summary>
        /// DeleteCommand
        /// </summary>
        public static RoutedUICommand Delete
        {
            get { return _EnsureCommand(CommandId.Delete); }
        }

        /// <summary>
        /// UndoCommand
        /// </summary>
        public static RoutedUICommand Undo
        {
            get { return _EnsureCommand(CommandId.Undo); }
        }

        /// <summary>
        /// RedoCommand
        /// </summary>
        public static RoutedUICommand Redo
        {
            get { return _EnsureCommand(CommandId.Redo); }
        }

        /// <summary>
        /// SelectAllCommand
        /// </summary>
        public static RoutedUICommand Find
        {
            get { return _EnsureCommand(CommandId.Find); }
        }

        /// <summary>
        /// ReplaceCommand
        /// </summary>
        public static RoutedUICommand Replace
        {
            get { return _EnsureCommand(CommandId.Replace); }
        }

        /// <summary>
        /// SelectAllCommand
        /// </summary>
        public static RoutedUICommand SelectAll
        {
            get { return _EnsureCommand(CommandId.SelectAll); }
        }

        /// <summary>
        /// HelpCommand
        /// </summary>
        public static RoutedUICommand Help
        {
            get { return _EnsureCommand(CommandId.Help); }
        }

        /// <summary>
        /// NewCommand
        /// </summary>
        public static RoutedUICommand New
        {
            get { return _EnsureCommand(CommandId.New); }
        }

        /// <summary>
        /// OpenCommand
        /// </summary>
        public static RoutedUICommand Open
        {
            get { return _EnsureCommand(CommandId.Open); }
        }

        /// <summary>
        /// CloseCommand
        /// </summary>
        public static RoutedUICommand Close
        {
            get { return _EnsureCommand(CommandId.Close); }
        }


        /// <summary>
        /// SaveCommand
        /// </summary>
        public static RoutedUICommand Save
        {
            get { return _EnsureCommand(CommandId.Save); }
        }

        /// <summary>
        /// SaveAsCommand
        /// </summary>
        public static RoutedUICommand SaveAs
        {
            get { return _EnsureCommand(CommandId.SaveAs); }
        }

        /// <summary>
        /// PrintCommand
        /// </summary>
        public static RoutedUICommand Print
        {
            get { return _EnsureCommand(CommandId.Print); }
        }

        /// <summary>
        /// CancelPrintCommand
        /// </summary>
        public static RoutedUICommand CancelPrint
        {
            get { return _EnsureCommand(CommandId.CancelPrint); }
        }

        /// <summary>
        /// PrintPreviewCommand
        /// </summary>
        public static RoutedUICommand PrintPreview
        {
            get { return _EnsureCommand(CommandId.PrintPreview); }
        }

        /// <summary>
        /// PropertiesCommand
        /// </summary>
        public static RoutedUICommand Properties
        {
            get { return _EnsureCommand(CommandId.Properties); }
        }

        /// <summary>
        /// ContextMenuCommand
        /// </summary>
        public static RoutedUICommand ContextMenu
        {
            get { return _EnsureCommand(CommandId.ContextMenu); }
        }

        /// <summary>
        /// StopCommand
        /// </summary>
        public static RoutedUICommand Stop
        {
            get { return _EnsureCommand(CommandId.Stop); }
        }

        /// <summary>
        /// CorrectionListCommand
        /// </summary>
        public static RoutedUICommand CorrectionList
        {
            get { return _EnsureCommand(CommandId.CorrectionList); }
        }

        /// <summary>
        /// NotACommand command.
        /// </summary>
        /// <remarks>
        /// This "command" is always ignored, without handling the input event
        /// that caused it.  This provides a way to turn off an input binding
        /// built into an existing control.
        /// </remarks>
        public static RoutedUICommand NotACommand
        {
            get { return _EnsureCommand(CommandId.NotACommand); }
        }
#endregion Public Methods

        //------------------------------------------------------
        //
        //  Private Methods
        //
        //------------------------------------------------------
#region Private Methods


        private static string GetPropertyName(CommandId commandId)
        {
            string propertyName = String.Empty;

            switch (commandId)
            {
                case CommandId.Cut: propertyName = "Cut"; break;
                case CommandId.Copy: propertyName = "Copy"; break;
                case CommandId.Paste: propertyName = "Paste"; break;
                case CommandId.Undo: propertyName = "Undo"; break;
                case CommandId.Redo: propertyName = "Redo"; break;
                case CommandId.Delete: propertyName = "Delete"; break;
                case CommandId.Find: propertyName = "Find"; break;
                case CommandId.Replace: propertyName = "Replace"; break;
                case CommandId.Help: propertyName = "Help"; break;
                case CommandId.New: propertyName = "New"; break;
                case CommandId.Open: propertyName = "Open"; break;
                case CommandId.Save: propertyName = "Save"; break;
                case CommandId.SaveAs: propertyName = "SaveAs"; break;
                case CommandId.Close: propertyName = "Close"; break;
                case CommandId.Print: propertyName = "Print"; break;
                case CommandId.CancelPrint: propertyName = "CancelPrint"; break;
                case CommandId.PrintPreview: propertyName = "PrintPreview"; break;
                case CommandId.Properties: propertyName = "Properties"; break;
                case CommandId.ContextMenu: propertyName = "ContextMenu"; break;
                case CommandId.CorrectionList: propertyName = "CorrectionList"; break;
                case CommandId.SelectAll: propertyName = "SelectAll"; break;
                case CommandId.Stop: propertyName = "Stop"; break;
                case CommandId.NotACommand: propertyName = "NotACommand"; break;
            }
            return propertyName;
        }

        internal static string GetUIText(byte commandId)
        {
            string uiText = String.Empty;

            switch ((CommandId)commandId)
            {
                case  CommandId.Cut: uiText = SR.CutText; break;
                case  CommandId.Copy: uiText = SR.CopyText;break;
                case  CommandId.Paste: uiText = SR.PasteText;break;
                case  CommandId.Undo: uiText = SR.UndoText;break;
                case  CommandId.Redo: uiText =  SR.RedoText; break;
                case  CommandId.Delete: uiText =  SR.DeleteText; break;
                case  CommandId.Find: uiText =  SR.FindText; break;
                case  CommandId.Replace: uiText =  SR.ReplaceText; break;
                case  CommandId.SelectAll: uiText =  SR.SelectAllText; break;
                case  CommandId.Help: uiText =  SR.HelpText; break;
                case  CommandId.New: uiText =  SR.NewText; break;
                case  CommandId.Open: uiText =  SR.OpenText; break;
                case  CommandId.Save: uiText =  SR.SaveText; break;
                case  CommandId.SaveAs: uiText =  SR.SaveAsText; break;
                case  CommandId.Print: uiText =  SR.PrintText; break;
                case  CommandId.CancelPrint: uiText =  SR.CancelPrintText; break;
                case  CommandId.PrintPreview: uiText =  SR.PrintPreviewText; break;
                case  CommandId.Close: uiText =  SR.CloseText; break;
                case  CommandId.ContextMenu: uiText =  SR.ContextMenuText; break;
                case  CommandId.CorrectionList: uiText =  SR.CorrectionListText; break;
                case  CommandId.Properties: uiText =  SR.PropertiesText; break;
                case  CommandId.Stop: uiText =  SR.StopText; break;
                case  CommandId.NotACommand: uiText =  SR.NotACommandText; break;
            }

            return uiText;
        }

        internal static InputGestureCollection LoadDefaultGestureFromResource(byte commandId)
        {
            InputGestureCollection gestures = new InputGestureCollection();

            //Standard Commands
            switch ((CommandId)commandId)
            {
                case  CommandId.Cut:
                    KeyGesture.AddGesturesFromResourceStrings(
                        CutKey,
                        SR.CutKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Copy:
                    KeyGesture.AddGesturesFromResourceStrings(
                        CopyKey,
                        SR.CopyKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Paste:
                    KeyGesture.AddGesturesFromResourceStrings(
                        PasteKey,
                        SR.PasteKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Undo:
                    KeyGesture.AddGesturesFromResourceStrings(
                        UndoKey,
                        SR.UndoKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Redo:
                    KeyGesture.AddGesturesFromResourceStrings(
                        RedoKey,
                        SR.RedoKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Delete:
                    KeyGesture.AddGesturesFromResourceStrings(
                        DeleteKey,
                        SR.DeleteKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Find:
                    KeyGesture.AddGesturesFromResourceStrings(
                        FindKey,
                        SR.FindKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Replace:
                    KeyGesture.AddGesturesFromResourceStrings(
                        ReplaceKey,
                        SR.ReplaceKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.SelectAll:
                    KeyGesture.AddGesturesFromResourceStrings(
                        SelectAllKey,
                        SR.SelectAllKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Help:
                    KeyGesture.AddGesturesFromResourceStrings(
                        HelpKey,
                        SR.HelpKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.New:
                    KeyGesture.AddGesturesFromResourceStrings(
                        NewKey,
                        SR.NewKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Open:
                    KeyGesture.AddGesturesFromResourceStrings(
                        OpenKey,
                        SR.OpenKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Save:
                    KeyGesture.AddGesturesFromResourceStrings(
                        SaveKey,
                        SR.SaveKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.SaveAs:
                    break; // there are no default bindings for  CommandId.SaveAs
                case  CommandId.Print:
                    KeyGesture.AddGesturesFromResourceStrings(
                        PrintKey,
                        SR.PrintKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.CancelPrint:
                    break; // there are no default bindings for  CommandId.CancelPrint
                case  CommandId.PrintPreview:
                    KeyGesture.AddGesturesFromResourceStrings(
                        PrintPreviewKey,
                        SR.PrintPreviewKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Close:
                    break; // there are no default bindings for  CommandId.Close
                case  CommandId.ContextMenu:
                    KeyGesture.AddGesturesFromResourceStrings(
                        ContextMenuKey,
                        SR.ContextMenuKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.CorrectionList:
                    KeyGesture.AddGesturesFromResourceStrings(
                        SR.CorrectionListKey,
                        SR.CorrectionListKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Properties:
                    KeyGesture.AddGesturesFromResourceStrings(
                        PropertiesKey,
                        SR.PropertiesKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.Stop:
                    KeyGesture.AddGesturesFromResourceStrings(
                        StopKey,
                        SR.StopKeyDisplayString,
                        gestures);
                    break;
                case  CommandId.NotACommand:
                    break; // there are no default bindings for  CommandId.NotACommand
            }
            return gestures;
        }


      private static RoutedUICommand _EnsureCommand(CommandId idCommand)
        {
            if (idCommand >= 0 && idCommand < CommandId.Last)
            {
                lock (_internalCommands.SyncRoot)
                {
                    if (_internalCommands[(int)idCommand] == null)
                    {
                        RoutedUICommand newCommand = CommandLibraryHelper.CreateUICommand(
                            GetPropertyName(idCommand),
                            typeof(ApplicationCommands), (byte)idCommand);

                        _internalCommands[(int)idCommand] = newCommand;
                    }
                }
                return _internalCommands[(int)idCommand];
            }
            return null;
        }
#endregion Private Methods

        //------------------------------------------------------
        //
        //  Private Fields
        //
        //------------------------------------------------------
#region Private Fields
        // these constants will go away in future, its just to index into the right one.
        private enum CommandId : byte
        {
            Cut=0,
            Copy=1,
            Paste=2,
            Undo=3,
            Redo=4,
            Delete=5,
            Find=6,
            Replace=7,
            Help=8,
            SelectAll=9,
            New=10,
            Open=11,
            Save=12,
            SaveAs=13,
            Print = 14,
            CancelPrint = 15,
            PrintPreview = 16,
            Close = 17,
            Properties=18,
            ContextMenu=19,
            CorrectionList=20,
            Stop=21,
            NotACommand=22,

            // Last
            Last=23
        }

        private static RoutedUICommand[] _internalCommands = new RoutedUICommand[(int)CommandId.Last];
#endregion Private Fields

        private const string ContextMenuKey = "Shift+F10;Apps";
        private const string CopyKey = "Ctrl+C;Ctrl+Insert";
        private const string CutKey = "Ctrl+X;Shift+Delete";
        private const string DeleteKey = "Del";
        private const string FindKey = "Ctrl+F";
        private const string HelpKey = "F1";
        private const string NewKey = "Ctrl+N";
        private const string OpenKey = "Ctrl+O";
        private const string PasteKey = "Ctrl+V;Shift+Insert";
        private const string PrintKey = "Ctrl+P";
        private const string PrintPreviewKey = "Ctrl+F2";
        private const string PropertiesKey = "F4";
        private const string RedoKey = "Ctrl+Y";
        private const string ReplaceKey = "Ctrl+H";
        private const string SaveKey = "Ctrl+S";
        private const string SelectAllKey = "Ctrl+A";
        private const string StopKey = "Esc";
        private const string UndoKey = "Ctrl+Z";
    }
}
