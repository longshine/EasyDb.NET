//
// LX.EasyDb.Dialects.Function.TemplateRenderer.cs
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

namespace LX.EasyDb.Dialects.Function
{
    class TemplateRenderer
    {
        private String[] _chunks;
        private Int32[] _paramIndexes;

        public TemplateRenderer(String template)
        {
            this.Template = template;

            List<String> chunkList = new List<String>();
            List<Int32> paramList = new List<Int32>();
            StringBuilder chunk = new StringBuilder(10);
            StringBuilder index = new StringBuilder(2);

            for (Int32 i = 0; i < template.Length; ++i)
            {
                Char c = template[i];
                if (c == '?')
                {
                    chunkList.Add(chunk.ToString());
                    chunk.Remove(0, chunk.Length);

                    while (++i < template.Length)
                    {
                        c = template[i];
                        if (Char.IsDigit(c))
                        {
                            index.Append(c);
                        }
                        else
                        {
                            chunk.Append(c);
                            break;
                        }
                    }

                    paramList.Add(Int32.Parse(index.ToString()));
                    index.Remove(0, index.Length);
                }
                else
                {
                    chunk.Append(c);
                }
            }

            if (chunk.Length > 0)
            {
                chunkList.Add(chunk.ToString());
            }

            _chunks = chunkList.ToArray();
            _paramIndexes = new Int32[paramList.Count];
            for (Int32 i = 0; i < _paramIndexes.Length; ++i)
            {
                _paramIndexes[i] = paramList[i];
            }
        }

        public String Template { get; set; }

        public Int32 AnticipatedNumberOfArguments
        {
            get { return _paramIndexes.Length; }
        }

        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            int numberOfArguments = args.Count;
            if (AnticipatedNumberOfArguments > 0 && numberOfArguments != AnticipatedNumberOfArguments)
            {
                //log.warn( "Function template anticipated {} arguments, but {} arguments encountered",
                //getAnticipatedNumberOfArguments(), numberOfArguments );
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _chunks.Length; ++i)
            {
                if (i < _paramIndexes.Length)
                {
                    int index = _paramIndexes[i] - 1;
                    Object arg = index < numberOfArguments ? args[index] : null;
                    if (arg != null)
                    {
                        sb.Append(_chunks[i]).Append(arg);
                    }
                }
                else
                {
                    sb.Append(_chunks[i]);
                }
            }
            return sb.ToString();
        }
    }
}
