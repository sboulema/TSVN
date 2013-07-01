namespace FundaRealEstateBV.TSVN
{
    static class PkgCmdIdList
    {
        public const uint UpdateCommand = 0x100;
        public const uint CommitCommand = 0x200;
        public const uint ShowChangesCommand = 0x300;
        public const uint ShowLogCommand = 0x400;
        public const uint CreatePatchCommand = 0x500;
        public const uint ApplyPatchCommand = 0x600;
        public const uint RevertCommand = 0x700;
        public const uint DiskBrowser = 0x800;
        public const uint RepoBrowser = 0x900;
        public const uint BranchCommand = 0x910;
        public const uint SwitchCommand = 0x920;
        public const uint MergeCommand = 0x930;
    };
}