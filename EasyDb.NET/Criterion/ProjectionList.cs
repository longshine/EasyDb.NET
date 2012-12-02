//
// LX.EasyDb.Criterion.ProjectionList.cs
//
// Authors:
//	Longshine He <longshinehe@users.sourceforge.net>
//
// Copyright (c) 2012 Longshine He
//
// This code is distributed in the hope that it will be useful,
// but WITHOUT WARRANTY OF ANY KIND.
//

using System;
using System.Collections.Generic;
using System.Text;

namespace LX.EasyDb.Criterion
{
    /// <summary>
    /// Represents a collection of projections.
    /// </summary>
    public class ProjectionList : IProjection
    {
        private List<IProjection> _projections = new List<IProjection>();

        /// <summary>
        /// Gets or sets the alias of this projection.
        /// </summary>
        public String Alias { get; set; }

        /// <summary>
        /// Checks if this projection is grouped.
        /// </summary>
        public Boolean Grouped
        {
            get
            {
                foreach (IProjection p in _projections)
                {
                    if (p.Grouped)
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Adds a projection.
        /// </summary>
        public ProjectionList Add(IProjection projection)
        {
            _projections.Add(projection);
            return this;
        }

        /// <summary>
        /// Adds a projection with the given alias.
        /// </summary>
        public ProjectionList Add(IProjection projection, String alias)
        {
            projection.Alias = alias;
            _projections.Add(projection);
            return this;
        }

        /// <summary>
        /// Renders this projection.
        /// </summary>
        public String Render(ICriteria criteria)
        {
            StringBuilder sb = StringHelper.CreateBuilder();
            Boolean appendSeperator = false;
            foreach (IProjection p in _projections)
            {
                if (appendSeperator)
                    sb.Append(", ");
                else
                    appendSeperator = true;
                sb.Append(p.Render(criteria));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Gets the grouping string of this projection if grouped.
        /// </summary>
        public String ToGroupString(ICriteria criteria)
        {
            List<String> groups = new List<String>(_projections.Count);
            foreach (IProjection p in _projections)
            {
                if (p.Grouped)
                    groups.Add(p.ToGroupString(criteria));
            }
            return String.Join(", ", groups.ToArray());
        }
    }
}
