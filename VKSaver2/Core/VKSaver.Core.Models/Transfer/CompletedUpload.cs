using System;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public sealed class CompletedUpload : ICompletedUpload
    {
        public Guid Id { get; set; }

        public FileContentType ContentType { get; set; }

        public string Name { get; set; }

        public string ServerResponse { get; set; }
    }
}
