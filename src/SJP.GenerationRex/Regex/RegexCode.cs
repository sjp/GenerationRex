// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace SJP.GenerationRex.RegularExpressions;

internal static class RegexCode
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
}