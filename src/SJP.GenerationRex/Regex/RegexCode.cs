// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This RegexCode class is internal to the regular expression package.
// It provides operator constants for use by the Builder and the Machine.

// Implementation notes:
//
// Regexps are built into RegexCodes, which contain an operation array,
// a string table, and some constants.
//
// Each operation is one of the codes below, followed by the integer
// operands specified for each op.
//
// Strings and sets are indices into a string table.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SJP.GenerationRex.RegularExpressions
{
    internal sealed class RegexCode
    {
        // The following primitive operations come directly from the parser

                                                    // lef/back operands        description
        internal const int Onerep = 0;              // lef,back char,min,max    a {n}
        internal const int Notonerep = 1;           // lef,back char,min,max    .{n}
        internal const int Setrep = 2;              // lef,back set,min,max     [\d]{n}

        internal const int Oneloop = 3;             // lef,back char,min,max    a {,n}
        internal const int Notoneloop = 4;          // lef,back char,min,max    .{,n}
        internal const int Setloop = 5;             // lef,back set,min,max     [\d]{,n}

        internal const int Onelazy = 6;             // lef,back char,min,max    a {,n}?
        internal const int Notonelazy = 7;          // lef,back char,min,max    .{,n}?
        internal const int Setlazy = 8;             // lef,back set,min,max     [\d]{,n}?

        internal const int One = 9;                 // lef      char            a
        internal const int Notone = 10;             // lef      char            [^a]
        internal const int Set = 11;                // lef      set             [a-z\s]  \w \s \d

        internal const int Multi = 12;              // lef      string          abcd
        internal const int Ref = 13;                // lef      group           \#

        internal const int Bol = 14;                //                          ^
        internal const int Eol = 15;                //                          $
        internal const int Boundary = 16;           //                          \b
        internal const int Nonboundary = 17;        //                          \B
        internal const int Beginning = 18;          //                          \A
        internal const int Start = 19;              //                          \G
        internal const int EndZ = 20;               //                          \Z
        internal const int End = 21;                //                          \Z

        internal const int Nothing = 22;            //                          Reject!

        // Primitive control structures

        internal const int Lazybranch = 23;         // back     jump            straight first
        internal const int Branchmark = 24;         // back     jump            branch first for loop
        internal const int Lazybranchmark = 25;     // back     jump            straight first for loop
        internal const int Nullcount = 26;          // back     val             set counter, null mark
        internal const int Setcount = 27;           // back     val             set counter, make mark
        internal const int Branchcount = 28;        // back     jump,limit      branch++ if zero<=c<limit
        internal const int Lazybranchcount = 29;    // back     jump,limit      same, but straight first
        internal const int Nullmark = 30;           // back                     save position
        internal const int Setmark = 31;            // back                     save position
        internal const int Capturemark = 32;        // back     group           define group
        internal const int Getmark = 33;            // back                     recall position
        internal const int Setjump = 34;            // back                     save backtrack state
        internal const int Backjump = 35;           //                          zap back to saved state
        internal const int Forejump = 36;           //                          zap backtracking state
        internal const int Testref = 37;            //                          backtrack if ref undefined
        internal const int Goto = 38;               //          jump            just go

        internal const int Prune = 39;              //                          prune it baby
        internal const int Stop = 40;               //                          done!

        internal const int ECMABoundary = 41;       //                          \b
        internal const int NonECMABoundary = 42;    //                          \B

        // Modifiers for alternate modes
        internal const int Mask = 63;   // Mask to get unmodified ordinary operator
        internal const int Rtl = 64;    // bit to indicate that we're reverse scanning.
        internal const int Back = 128;  // bit to indicate that we're backtracking.
        internal const int Back2 = 256; // bit to indicate that we're backtracking on a second branch.
        internal const int Ci = 512;    // bit to indicate that we're case-insensitive.

        internal readonly int[] _codes;                     // the code
        internal readonly string[] _strings;                // the string/set table
        internal readonly int _trackcount;                  // how many instructions use backtracking
        internal readonly Hashtable _caps;                  // mapping of user group numbers -> impl group slots
        internal readonly int _capsize;                     // number of impl group slots
        internal readonly RegexPrefix _fcPrefix;            // the set of candidate first characters (may be null)
        internal readonly RegexBoyerMoore _bmPrefix;        // the fixed prefix string as a Boyer-Moore machine (may be null)
        internal readonly int _anchors;                     // the set of zero-length start anchors (RegexFCD.Bol, etc)
        internal readonly bool _rightToLeft;                // true if right to left

        internal RegexCode(int[] codes, List<string> stringlist, int trackcount,
                           Hashtable caps, int capsize,
                           RegexBoyerMoore bmPrefix, RegexPrefix fcPrefix,
                           int anchors, bool rightToLeft)
        {
            Debug.Assert(codes != null, "codes cannot be null.");
            Debug.Assert(stringlist != null, "stringlist cannot be null.");

            _codes = codes;
            _strings = stringlist.ToArray();
            _trackcount = trackcount;
            _caps = caps;
            _capsize = capsize;
            _bmPrefix = bmPrefix;
            _fcPrefix = fcPrefix;
            _anchors = anchors;
            _rightToLeft = rightToLeft;
        }

        internal static bool OpcodeBacktracks(int Op)
        {
            Op &= Mask;

            switch (Op)
            {
                case Oneloop:
                case Notoneloop:
                case Setloop:
                case Onelazy:
                case Notonelazy:
                case Setlazy:
                case Lazybranch:
                case Branchmark:
                case Lazybranchmark:
                case Nullcount:
                case Setcount:
                case Branchcount:
                case Lazybranchcount:
                case Setmark:
                case Capturemark:
                case Getmark:
                case Setjump:
                case Backjump:
                case Forejump:
                case Goto:
                    return true;

                default:
                    return false;
            }
        }
    }
}
