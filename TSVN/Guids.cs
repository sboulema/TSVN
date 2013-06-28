// Guids.cs
// MUST match guids.h
using System;

namespace FundaRealEstateBV.TSVN
{
    static class GuidList
    {
        public const string guidTSVNPkgString = "f2e68d5a-c95e-4d53-bbc6-072ff3ed9c53";
        public const string guidTSVNCmdSetString = "65ab72da-3aa4-4e36-8182-b3dbe7ff6b56";

        public static readonly Guid guidTSVNCmdSet = new Guid(guidTSVNCmdSetString);
    };
}