using System;
using System.Collections.Generic;
using System.Linq;
using Mentoring.Lab8.Task1.Downloader.Models;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public class FileTypesConstraint : IConstraint
    {
        private readonly IEnumerable<string> _allowedExtensions;

        public FileTypesConstraint(IEnumerable<string> allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        public ConstraintType ConstraintType
        {
            get => ConstraintType.File;
        }

        public bool IsValid(Uri uri)
        {
            var lastSegment = uri.Segments.Last();

            if (lastSegment == null)
            {
                return false;
            }

            return _allowedExtensions.Any(ae => lastSegment.EndsWith(ae));
        }
    }
}
