using System;
using System.Collections.Generic;

namespace LX.EasyDb.Dialects.Function
{
    /// <summary>
    /// Abstract base class for ansi trim emulation functions.
    /// </summary>
    public abstract class AbstractAnsiTrimEmulationFunction : ISQLFunction
    {
        /// <inheritdoc/>
        public DbType GetReturnType(DbType firstArgumentType)
        {
            return DbType.String;
        }

        /// <inheritdoc/>
        public String Render(IList<Object> args, IConnectionFactory factory)
        {
            // According to both the ANSI-SQL and JPA specs, trim takes a variable number of parameters between 1 and 4.
            // at least one paramer (trimSource) is required.  From the SQL spec:
            //
            // <trim function> ::=
            //      TRIM <left paren> <trim operands> <right paren>
            //
            // <trim operands> ::=
            //      [ [ <trim specification> ] [ <trim character> ] FROM ] <trim source>
            //
            // <trim specification> ::=
            //      LEADING
            //      | TRAILING
            //      | BOTH
            //
            // If <trim specification> is omitted, BOTH is assumed.
            // If <trim character> is omitted, space is assumed
            if (args.Count == 1)
            {
                // we have the form: trim(trimSource)
                //      so we trim leading and trailing spaces
                return ResolveBothSpaceTrimFunction().Render(args, factory);			// EARLY EXIT!!!!
            }
            else if ("from".Equals((String)args[0], StringComparison.OrdinalIgnoreCase))
            {
                // we have the form: trim(from trimSource).
                //      This is functionally equivalent to trim(trimSource)
                return ResolveBothSpaceTrimFromFunction().Render(args, factory);  		// EARLY EXIT!!!!
            }
            else
            {
                // otherwise, a trim-specification and/or a trim-character
                // have been specified;  we need to decide which options
                // are present and "do the right thing"
                Boolean leading = true;         // should leading trim-characters be trimmed?
                Boolean trailing = true;        // should trailing trim-characters be trimmed?
                String trimCharacter;    		// the trim-character (what is to be trimmed off?)
                String trimSource;       		// the trim-source (from where should it be trimmed?)

                // potentialTrimCharacterArgIndex = 1 assumes that a
                // trim-specification has been specified.  we handle the
                // exception to that explicitly
                int potentialTrimCharacterArgIndex = 1;
                String firstArg = (String)args[0];
                if ("leading".Equals(firstArg, StringComparison.OrdinalIgnoreCase))
                {
                    trailing = false;
                }
                else if ("trailing".Equals(firstArg, StringComparison.OrdinalIgnoreCase))
                {
                    leading = false;
                }
                else if ("both".Equals(firstArg, StringComparison.OrdinalIgnoreCase))
                {
                }
                else
                {
                    potentialTrimCharacterArgIndex = 0;
                }

                String potentialTrimCharacter = (String)args[potentialTrimCharacterArgIndex];
                if ("from".Equals(potentialTrimCharacter, StringComparison.OrdinalIgnoreCase))
                {
                    trimCharacter = "' '";
                    trimSource = (String)args[potentialTrimCharacterArgIndex + 1];
                }
                else if (potentialTrimCharacterArgIndex + 1 >= args.Count)
                {
                    trimCharacter = "' '";
                    trimSource = potentialTrimCharacter;
                }
                else
                {
                    trimCharacter = potentialTrimCharacter;
                    if ("from".Equals((String)args[potentialTrimCharacterArgIndex + 1], StringComparison.OrdinalIgnoreCase))
                    {
                        trimSource = (String)args[potentialTrimCharacterArgIndex + 2];
                    }
                    else
                    {
                        trimSource = (String)args[potentialTrimCharacterArgIndex + 1];
                    }
                }

                List<Object> argsToUse = new List<Object>();
                argsToUse.Add(trimSource);
                argsToUse.Add(trimCharacter);

                if (trimCharacter.Equals("' '"))
                {
                    if (leading && trailing)
                    {
                        return ResolveBothSpaceTrimFunction().Render(argsToUse, factory);
                    }
                    else if (leading)
                    {
                        return ResolveLeadingSpaceTrimFunction().Render(argsToUse, factory);
                    }
                    else
                    {
                        return ResolveTrailingSpaceTrimFunction().Render(argsToUse, factory);
                    }
                }
                else
                {
                    if (leading && trailing)
                    {
                        return ResolveBothTrimFunction().Render(argsToUse, factory);
                    }
                    else if (leading)
                    {
                        return ResolveLeadingTrimFunction().Render(argsToUse, factory);
                    }
                    else
                    {
                        return ResolveTrailingTrimFunction().Render(argsToUse, factory);
                    }
                }
            }
        }

        /// <summary>
        /// Resolve the function definition which should be used to trim both leading and trailing spaces.
        /// In this form, the imput arguments is missing the <tt>FROM</tt> keyword.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveBothSpaceTrimFunction();

        /// <summary>
        /// Resolve the function definition which should be used to trim both leading and trailing spaces.
        /// The same as <see cref="ResolveBothSpaceTrimFunction"/> except that here the<tt>FROM</tt> is included and
        /// will need to be accounted for during render processing.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveBothSpaceTrimFromFunction();

        /// <summary>
        /// esolve the function definition which should be used to trim leading spaces.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveLeadingSpaceTrimFunction();

        /// <summary>
        /// Resolve the function definition which should be used to trim trailing spaces.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveTrailingSpaceTrimFunction();

        /// <summary>
        /// Resolve the function definition which should be used to trim the specified character from both the
        /// beginning (leading) and end (trailing) of the trim source.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveBothTrimFunction();

        /// <summary>
        /// Resolve the function definition which should be used to trim the specified character from the
        /// beginning (leading) of the trim source.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveLeadingTrimFunction();

        /// <summary>
        /// Resolve the function definition which should be used to trim the specified character from the
        /// end (trailing) of the trim source.
        /// </summary>
        /// <returns>The sql function</returns>
        protected abstract ISQLFunction ResolveTrailingTrimFunction();
    }
}
