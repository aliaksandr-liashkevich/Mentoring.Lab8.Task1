using System;
using Mentoring.Lab8.Task1.Downloader.Models;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public interface IConstraint
    {
        ConstraintType ConstraintType { get; }
        bool IsValid(Uri uri);
    }
}
