using System;

namespace Mentoring.Lab8.Task1.Downloader.Models
{
    [Flags]
    public enum ConstraintType
    {
        None = 0,
        Url = 1,
        File = 2
    }
}
