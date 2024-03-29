﻿using System.Collections.Generic;
using VKSaver.Core.Models.Common;

namespace VKSaver.Core.Models.Transfer
{
    public class SimpleUploadable : IUploadable
    {
        public string Name { get; set; }

        public string Extension { get; set; }
        public Dictionary<string, string> AdditionalData { get; set; }

        public FileContentType ContentType { get; set; }

        public IDataSource Source { get; set; }
    }
}
