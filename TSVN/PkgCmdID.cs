namespace FundaRealEstateBV.TSVN
{
    static class PkgCmdIdList
    {
        public const uint ShowChangesCommand = 0x0100;
        public const uint UpdateCommand = 0x0101;
        public const uint CommitCommand = 0x0102;
        
        public const uint ShowLogCommand = 0x0200;
        public const uint DiskBrowser = 0x0201;
        public const uint RepoBrowser = 0x0202;

        public const uint CreatePatchCommand = 0x0300;
        public const uint ApplyPatchCommand = 0x0301;

        public const uint BranchCommand = 0x0400;
        public const uint SwitchCommand = 0x0401;
        public const uint MergeCommand = 0x0402;

        public const uint RevertCommand = 0x0500;
        public const uint UpdateToRevisionCommand = 0x0501;
        public const uint CleanupCommand = 0x0502;    
        
        public const uint DifferencesCommand = 0x0600;
        public const uint BlameCommand = 0x0601;
        public const uint ShowLogFileCommand = 0x0602;
        public const uint DiskBrowserFileCommand = 0x0603;
        public const uint RepoBrowserFileCommand = 0x0604;
    };
}