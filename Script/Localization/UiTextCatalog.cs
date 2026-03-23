using ExShrinkSidebar.Asset.Properties;
using System.Collections.Generic;

namespace ExShrinkSidebar.Script.Localization
{
    public static class UiTextCatalog
    {
        public const string ConfigEditorTitle = "Ui_ConfigEditorTitle";
        public const string NewConfigName = "Ui_NewConfigName";
        public const string NewStepName = "Ui_NewStepName";
        public const string ConfigNameLabel = "Ui_ConfigNameLabel";
        public const string FunctionLabel = "Ui_FunctionLabel";
        public const string SummarySteps = "Ui_SummarySteps";
        public const string SummaryOpenFolder = "Ui_SummaryOpenFolder";
        public const string SummaryOpenFolderWithCorner = "Ui_SummaryOpenFolderWithCorner";
        public const string SummaryExecute = "Ui_SummaryExecute";
        public const string SummaryNotConfiguredFolder = "Ui_SummaryNotConfiguredFolder";
        public const string SummaryNotConfiguredExecute = "Ui_SummaryNotConfiguredExecute";
        public const string SummaryNotepad = "Ui_SummaryNotepad";
        public const string AddRootTooltip = "Ui_AddRootTooltip";
        public const string RootCardTitle = "Ui_RootCardTitle";
        public const string SaveAndClose = "Ui_SaveAndClose";
        public const string ScriptEditorEntry = "Ui_ScriptEditorEntry";
        public const string TrayLanguageMenu = "Ui_TrayLanguageMenu";
        public const string LanguageZhCn = "Ui_LanguageZhCn";
        public const string LanguageEnUs = "Ui_LanguageEnUs";
        public const string TrayExit = "Ui_TrayExit";
        public const string DetailWindowSuffix = "Ui_DetailWindowSuffix";
        public const string DetailFolderPath = "Ui_DetailFolderPath";
        public const string DetailExecuteTarget = "Ui_DetailExecuteTarget";
        public const string DetailEnablePlacement = "Ui_DetailEnablePlacement";
        public const string DetailBrowse = "Ui_DetailBrowse";
        public const string DetailArguments = "Ui_DetailArguments";
        public const string DetailCorner = "Ui_DetailCorner";
        public const string DetailWidth = "Ui_DetailWidth";
        public const string DetailHeight = "Ui_DetailHeight";
        public const string DetailCancel = "Ui_DetailCancel";
        public const string DetailConfirm = "Ui_DetailConfirm";
        public const string DetailHintOpenFolder = "Ui_DetailHintOpenFolder";
        public const string DetailHintExecute = "Ui_DetailHintExecute";
        public const string DetailHintNone = "Ui_DetailHintNone";
        public const string FileDialogAllFiles = "Ui_FileDialogAllFiles";

        private static readonly Dictionary<string, string> Fallbacks = new Dictionary<string, string>
        {
            [ConfigEditorTitle] = "\u811A\u672C\u914D\u7F6E\u7F16\u8F91\u5668",
            [NewConfigName] = "\u65B0\u914D\u7F6E",
            [NewStepName] = "\u65B0\u6B65\u9AA4",
            [ConfigNameLabel] = "\u914D\u7F6E\u540D\u79F0\uFF1A",
            [FunctionLabel] = "\u529F\u80FD\uFF1A",
            [SummarySteps] = "\u6B65\u9AA4: {0}",
            [SummaryOpenFolder] = "\u76EE\u5F55: {0}",
            [SummaryOpenFolderWithCorner] = "\u76EE\u5F55: {0} | \u89D2\u843D: {1}",
            [SummaryExecute] = "\u6267\u884C: {0}",
            [SummaryNotConfiguredFolder] = "\u672A\u914D\u7F6E\u76EE\u5F55",
            [SummaryNotConfiguredExecute] = "\u672A\u914D\u7F6E\u6267\u884C\u76EE\u6807",
            [SummaryNotepad] = "\u76F4\u63A5\u6253\u5F00\u8BB0\u4E8B\u672C",
            [AddRootTooltip] = "\u65B0\u589E\u6839\u914D\u7F6E",
            [RootCardTitle] = "\u6839\u914D\u7F6E",
            [SaveAndClose] = "\u4FDD\u5B58\u5E76\u5173\u95ED",
            [ScriptEditorEntry] = "\u811A\u672C\u7F16\u8F91",
            [TrayLanguageMenu] = "\u8BED\u8A00",
            [LanguageZhCn] = "\u4E2D\u6587(\u7B80\u4F53)",
            [LanguageEnUs] = "English",
            [TrayExit] = "\u9000\u51FA",
            [DetailWindowSuffix] = "\u989D\u5916\u53C2\u6570",
            [DetailFolderPath] = "\u76EE\u5F55\u8DEF\u5F84",
            [DetailExecuteTarget] = "\u6267\u884C\u76EE\u6807",
            [DetailEnablePlacement] = "\u6253\u5F00\u76EE\u5F55\u540E\u8C03\u6574\u5230\u5C4F\u5E55\u89D2\u843D",
            [DetailBrowse] = "\u6D4F\u89C8",
            [DetailArguments] = "\u53C2\u6570",
            [DetailCorner] = "\u89D2\u843D\u4F4D\u7F6E",
            [DetailWidth] = "\u5BBD",
            [DetailHeight] = "\u9AD8",
            [DetailCancel] = "\u53D6\u6D88",
            [DetailConfirm] = "\u786E\u5B9A",
            [DetailHintOpenFolder] = "\u53EF\u914D\u7F6E\u6253\u5F00\u7684\u76EE\u5F55\u8DEF\u5F84\uFF0C\u5E76\u5728\u76EE\u5F55\u7A97\u53E3\u5F39\u51FA\u540E\u6309\u5C4F\u5E55\u56DB\u89D2\u548C\u5C3A\u5BF8\u8FDB\u884C\u91CD\u5B9A\u4F4D\u3002\u5BBD\u9AD8\u7559\u7A7A\u65F6\u9ED8\u8BA4\u4F7F\u7528\u5F53\u524D\u5C4F\u5E55\u7684\u4E00\u534A\u3002",
            [DetailHintExecute] = "Execute \u4F1A\u76F4\u63A5\u6253\u5F00\u76EE\u6807\u8DEF\u5F84\uFF0C\u5E76\u5C06\u53C2\u6570\u5B57\u7B26\u4E32\u539F\u6837\u4F20\u7ED9\u7CFB\u7EDF\u3002\u9002\u5408 exe\u3001bat\u3001cmd\u3001ps1 \u6216\u5176\u4ED6\u5DF2\u5173\u8054\u7684\u6587\u4EF6\u3002",
            [DetailHintNone] = "\u5F53\u524D\u52A8\u4F5C\u7C7B\u578B\u6CA1\u6709\u989D\u5916\u53C2\u6570\u3002",
            [FileDialogAllFiles] = "\u6240\u6709\u6587\u4EF6|*.*"
        };

        public static string Get(string key)
        {
            return StringResources.ResourceManager.GetString(key) ?? Fallbacks.GetValueOrDefault(key) ?? key;
        }

        public static string Format(string key, params object[] args)
        {
            return string.Format(Get(key), args);
        }
    }
}
