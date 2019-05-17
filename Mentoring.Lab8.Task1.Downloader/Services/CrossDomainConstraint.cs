using System;
using Mentoring.Lab8.Task1.Downloader.Models;

namespace Mentoring.Lab8.Task1.Downloader.Services
{
    public class CrossDomainConstraint : IConstraint
    {
        private readonly Uri _parentUri;
        private readonly CrossDomainConstraintType _crossDomainConstraintType;

        public CrossDomainConstraint(string parentUrl,
            CrossDomainConstraintType crossDomainConstraintType)
        {
            switch (crossDomainConstraintType)
            {
                case CrossDomainConstraintType.All:
                {
                    _crossDomainConstraintType = crossDomainConstraintType;
                    break;
                }
                case CrossDomainConstraintType.OnlyDescendantUrls:
                case CrossDomainConstraintType.OnlyCurrentDomain:
                {
                    if (string.IsNullOrEmpty(parentUrl))
                    {
                        throw new ArgumentNullException(nameof(parentUrl));
                    }

                    _parentUri = new Uri(parentUrl);
                    _crossDomainConstraintType = crossDomainConstraintType;
                    break;
                }
                default:
                    throw new ArgumentException($"Unknown cross domain constraint type: {crossDomainConstraintType}");

            }
        }

        public ConstraintType ConstraintType
        {
            get => ConstraintType.Url | ConstraintType.File;
        }

        public bool IsValid(Uri uri)
        {
            switch (_crossDomainConstraintType)
            {
                case CrossDomainConstraintType.All:
                    return true;
                case CrossDomainConstraintType.OnlyCurrentDomain:
                {
                    if (_parentUri.DnsSafeHost == uri.DnsSafeHost)
                    {
                        return true;
                    }

                    break;
                }
                case CrossDomainConstraintType.OnlyDescendantUrls:
                {
                    if (_parentUri.IsBaseOf(uri))
                    {
                        return true;
                    }

                    break;
                }
            }

            return false;
        }
    }
}
