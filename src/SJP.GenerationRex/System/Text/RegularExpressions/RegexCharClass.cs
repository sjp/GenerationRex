using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Text.RegularExpressions
{
    internal sealed class RegexCharClass
    {
        private static readonly string InternalRegexIgnoreCase = "__InternalRegexIgnoreCase__";
        private static readonly string Space = "d";
        private static readonly string NotSpace = RegexCharClass.NegateCategory(RegexCharClass.Space);
        private static readonly string[,] _propTable = new string[112, 2]
        {
      {
        "IsAlphabeticPresentationForms",
        "ﬀﭐ"
      },
      {
        "IsArabic",
        "\x0600܀"
      },
      {
        "IsArabicPresentationForms-A",
        "ﭐ︀"
      },
      {
        "IsArabicPresentationForms-B",
        "ﹰ\xFF00"
      },
      {
        "IsArmenian",
        "\x0530\x0590"
      },
      {
        "IsArrows",
        "←∀"
      },
      {
        "IsBasicLatin",
        "\0\x0080"
      },
      {
        "IsBengali",
        "ঀ\x0A00"
      },
      {
        "IsBlockElements",
        "▀■"
      },
      {
        "IsBopomofo",
        "\x3100\x3130"
      },
      {
        "IsBopomofoExtended",
        "ㆠ㇀"
      },
      {
        "IsBoxDrawing",
        "─▀"
      },
      {
        "IsBraillePatterns",
        "⠀⤀"
      },
      {
        "IsBuhid",
        "ᝀᝠ"
      },
      {
        "IsCJKCompatibility",
        "㌀㐀"
      },
      {
        "IsCJKCompatibilityForms",
        "︰﹐"
      },
      {
        "IsCJKCompatibilityIdeographs",
        "豈ﬀ"
      },
      {
        "IsCJKRadicalsSupplement",
        "⺀⼀"
      },
      {
        "IsCJKSymbolsandPunctuation",
        "　\x3040"
      },
      {
        "IsCJKUnifiedIdeographs",
        "一ꀀ"
      },
      {
        "IsCJKUnifiedIdeographsExtensionA",
        "㐀䷀"
      },
      {
        "IsCherokee",
        "Ꭰ᐀"
      },
      {
        "IsCombiningDiacriticalMarks",
        "̀Ͱ"
      },
      {
        "IsCombiningDiacriticalMarksforSymbols",
        "⃐℀"
      },
      {
        "IsCombiningHalfMarks",
        "︠︰"
      },
      {
        "IsCombiningMarksforSymbols",
        "⃐℀"
      },
      {
        "IsControlPictures",
        "␀⑀"
      },
      {
        "IsCurrencySymbols",
        "₠⃐"
      },
      {
        "IsCyrillic",
        "ЀԀ"
      },
      {
        "IsCyrillicSupplement",
        "Ԁ\x0530"
      },
      {
        "IsDevanagari",
        "ऀঀ"
      },
      {
        "IsDingbats",
        "✀⟀"
      },
      {
        "IsEnclosedAlphanumerics",
        "\x2460─"
      },
      {
        "IsEnclosedCJKLettersandMonths",
        "㈀㌀"
      },
      {
        "IsEthiopic",
        "ሀᎀ"
      },
      {
        "IsGeneralPunctuation",
        " \x2070"
      },
      {
        "IsGeometricShapes",
        "■☀"
      },
      {
        "IsGeorgian",
        "Ⴀᄀ"
      },
      {
        "IsGreek",
        "ͰЀ"
      },
      {
        "IsGreekExtended",
        "ἀ "
      },
      {
        "IsGreekandCoptic",
        "ͰЀ"
      },
      {
        "IsGujarati",
        "\x0A80\x0B00"
      },
      {
        "IsGurmukhi",
        "\x0A00\x0A80"
      },
      {
        "IsHalfwidthandFullwidthForms",
        "\xFF00\xFFF0"
      },
      {
        "IsHangulCompatibilityJamo",
        "\x3130㆐"
      },
      {
        "IsHangulJamo",
        "ᄀሀ"
      },
      {
        "IsHangulSyllables",
        "가ힰ"
      },
      {
        "IsHanunoo",
        "ᜠᝀ"
      },
      {
        "IsHebrew",
        "\x0590\x0600"
      },
      {
        "IsHighPrivateUseSurrogates",
        "\xDB80\xDC00"
      },
      {
        "IsHighSurrogates",
        "\xD800\xDB80"
      },
      {
        "IsHiragana",
        "\x3040゠"
      },
      {
        "IsIPAExtensions",
        "ɐʰ"
      },
      {
        "IsIdeographicDescriptionCharacters",
        "⿰　"
      },
      {
        "IsKanbun",
        "㆐ㆠ"
      },
      {
        "IsKangxiRadicals",
        "⼀\x2FE0"
      },
      {
        "IsKannada",
        "\x0C80\x0D00"
      },
      {
        "IsKatakana",
        "゠\x3100"
      },
      {
        "IsKatakanaPhoneticExtensions",
        "ㇰ㈀"
      },
      {
        "IsKhmer",
        "ក᠀"
      },
      {
        "IsKhmerSymbols",
        "᧠ᨀ"
      },
      {
        "IsLao",
        "\x0E80ༀ"
      },
      {
        "IsLatin-1Supplement",
        "\x0080Ā"
      },
      {
        "IsLatinExtended-A",
        "Āƀ"
      },
      {
        "IsLatinExtended-B",
        "ƀɐ"
      },
      {
        "IsLatinExtendedAdditional",
        "Ḁἀ"
      },
      {
        "IsLetterlikeSymbols",
        "℀\x2150"
      },
      {
        "IsLimbu",
        "ᤀᥐ"
      },
      {
        "IsLowSurrogates",
        "\xDC00\xE000"
      },
      {
        "IsMalayalam",
        "\x0D00\x0D80"
      },
      {
        "IsMathematicalOperators",
        "∀⌀"
      },
      {
        "IsMiscellaneousMathematicalSymbols-A",
        "⟀⟰"
      },
      {
        "IsMiscellaneousMathematicalSymbols-B",
        "⦀⨀"
      },
      {
        "IsMiscellaneousSymbols",
        "☀✀"
      },
      {
        "IsMiscellaneousSymbolsandArrows",
        "⬀Ⰰ"
      },
      {
        "IsMiscellaneousTechnical",
        "⌀␀"
      },
      {
        "IsMongolian",
        "᠀ᢰ"
      },
      {
        "IsMyanmar",
        "ကႠ"
      },
      {
        "IsNumberForms",
        "\x2150←"
      },
      {
        "IsOgham",
        " ᚠ"
      },
      {
        "IsOpticalCharacterRecognition",
        "⑀\x2460"
      },
      {
        "IsOriya",
        "\x0B00\x0B80"
      },
      {
        "IsPhoneticExtensions",
        "ᴀᶀ"
      },
      {
        "IsPrivateUse",
        "\xE000豈"
      },
      {
        "IsPrivateUseArea",
        "\xE000豈"
      },
      {
        "IsRunic",
        "ᚠᜀ"
      },
      {
        "IsSinhala",
        "\x0D80\x0E00"
      },
      {
        "IsSmallFormVariants",
        "﹐ﹰ"
      },
      {
        "IsSpacingModifierLetters",
        "ʰ̀"
      },
      {
        "IsSpecials",
        "\xFFF0"
      },
      {
        "IsSuperscriptsandSubscripts",
        "\x2070₠"
      },
      {
        "IsSupplementalArrows-A",
        "⟰⠀"
      },
      {
        "IsSupplementalArrows-B",
        "⤀⦀"
      },
      {
        "IsSupplementalMathematicalOperators",
        "⨀⬀"
      },
      {
        "IsSyriac",
        "܀ݐ"
      },
      {
        "IsTagalog",
        "ᜀᜠ"
      },
      {
        "IsTagbanwa",
        "ᝠក"
      },
      {
        "IsTaiLe",
        "ᥐᦀ"
      },
      {
        "IsTamil",
        "\x0B80ఀ"
      },
      {
        "IsTelugu",
        "ఀ\x0C80"
      },
      {
        "IsThaana",
        "ހ߀"
      },
      {
        "IsThai",
        "\x0E00\x0E80"
      },
      {
        "IsTibetan",
        "ༀက"
      },
      {
        "IsUnifiedCanadianAboriginalSyllabics",
        "᐀ "
      },
      {
        "IsVariationSelectors",
        "︀︐"
      },
      {
        "IsYiRadicals",
        "꒐ꓐ"
      },
      {
        "IsYiSyllables",
        "ꀀ꒐"
      },
      {
        "IsYijingHexagramSymbols",
        "䷀一"
      },
      {
        "_xmlC",
        "-/0;A[_`a{·¸À×Ø÷øĲĴĿŁŉŊſƀǄǍǱǴǶǺȘɐʩʻ˂ː˒̀͆͢͠Ά\x038BΌ\x038DΎ\x03A2ΣϏϐϗϚϛϜϝϞϟϠϡϢϴЁЍЎѐёѝў҂҃҇ҐӅӇӉӋӍӐӬӮӶӸӺԱ\x0557ՙ՚աևֺֻ֑֢֣־ֿ׀ׁ׃ׅׄא\x05EBװ׳ءػـٓ٠٪ٰڸںڿۀۏې۔ە۩۪ۮ۰ۺँऄअऺ़ॎ॑ॕक़।०॰ঁ\x0984অ\x098Dএ\x0991ও\x09A9প\x09B1ল\x09B3শ\x09BA়ঽা\x09C5ে\x09C9োৎৗ\x09D8ড়\x09DEয়\x09E4০৲ਂਃਅ\x0A0Bਏ\x0A11ਓ\x0A29ਪ\x0A31ਲ\x0A34ਵ\x0A37ਸ\x0A3A਼\x0A3Dਾ\x0A43ੇ\x0A49ੋ\x0A4Eਖ਼\x0A5Dਫ਼\x0A5F੦ੵઁ\x0A84અઌઍ\x0A8Eએ\x0A92ઓ\x0AA9પ\x0AB1લ\x0AB4વ\x0ABA઼\x0AC6ે\x0ACAો\x0ACEૠૡ૦૰ଁ\x0B04ଅ\x0B0Dଏ\x0B11ଓ\x0B29ପ\x0B31ଲ\x0B34ଶ\x0B3A଼ୄେ\x0B49ୋ\x0B4Eୖ\x0B58ଡ଼\x0B5Eୟୢ୦୰ஂ\x0B84அ\x0B8Bஎ\x0B91ஒ\x0B96ங\x0B9Bஜ\x0B9Dஞ\x0BA0ண\x0BA5ந\x0BABமஶஷ\x0BBAா\x0BC3ெ\x0BC9ொ\x0BCEௗ\x0BD8௧\x0BF0ఁ\x0C04అ\x0C0Dఎ\x0C11ఒ\x0C29పఴవ\x0C3Aా\x0C45ె\x0C49ొ\x0C4Eౕ\x0C57ౠౢ౦\x0C70ಂ\x0C84ಅ\x0C8Dಎ\x0C91ಒ\x0CA9ಪ\x0CB4ವ\x0CBAಾ\x0CC5ೆ\x0CC9ೊ\x0CCEೕ\x0CD7ೞ\x0CDFೠೢ೦\x0CF0ം\x0D04അ\x0D0Dഎ\x0D11ഒഩപഺാൄെ\x0D49ൊൎൗ\x0D58ൠൢ൦\x0D70กฯะ\x0E3Bเ๏๐๚ກ\x0E83ຄ\x0E85ງ\x0E89ຊ\x0E8Bຍ\x0E8Eດ\x0E98ນ\x0EA0ມ\x0EA4ລ\x0EA6ວ\x0EA8ສ\x0EACອຯະ\x0EBAົ\x0EBEເ\x0EC5ໆ\x0EC7່\x0ECE໐\x0EDA༘༚༠\x0F2A༵༶༷༸༹༺༾\x0F48ཉཪཱ྅྆ྌྐྖྗ\x0F98ྙྮྱྸྐྵྺႠ\x10C6აჷᄀᄁᄂᄄᄅᄈᄉᄊᄋᄍᄎᄓᄼᄽᄾᄿᅀᅁᅌᅍᅎᅏᅐᅑᅔᅖᅙᅚᅟᅢᅣᅤᅥᅦᅧᅨᅩᅪᅭᅯᅲᅴᅵᅶᆞᆟᆨᆩᆫᆬᆮᆰᆷᆹᆺᆻᆼᇃᇫᇬᇰᇱᇹᇺḀẜẠỺἀ\x1F16Ἐ\x1F1Eἠ\x1F46Ὀ\x1F4Eὐ\x1F58Ὑ\x1F5AὛ\x1F5CὝ\x1F5EὟ\x1F7Eᾀ\x1FB5ᾶ᾽ι᾿ῂ\x1FC5ῆ῍ῐ\x1FD4ῖ\x1FDCῠ῭ῲ\x1FF5ῶ´⃐⃝⃡⃢Ω℧Kℬ℮ℯↀↃ々〆〇〈〡〰〱〶ぁゕ゙゛ゝゟァ・ーヿㄅㄭ一龦가\xD7A4"
      },
      {
        "_xmlD",
        "0:٠٪۰ۺ०॰০ৰ੦ੰ૦૰୦୰௧\x0BF0౦\x0C70೦\x0CF0൦\x0D70๐๚໐\x0EDA༠\x0F2A၀၊\x1369\x1372០\x17EA᠐\x181A０："
      },
      {
        "_xmlI",
        ":;A[_`a{À×Ø÷øĲĴĿŁŉŊſƀǄǍǱǴǶǺȘɐʩʻ˂Ά·Έ\x038BΌ\x038DΎ\x03A2ΣϏϐϗϚϛϜϝϞϟϠϡϢϴЁЍЎѐёѝў҂ҐӅӇӉӋӍӐӬӮӶӸӺԱ\x0557ՙ՚աևא\x05EBװ׳ءػفًٱڸںڿۀۏې۔ەۖۥۧअऺऽाक़ॢঅ\x098Dএ\x0991ও\x09A9প\x09B1ল\x09B3শ\x09BAড়\x09DEয়ৢৰ৲ਅ\x0A0Bਏ\x0A11ਓ\x0A29ਪ\x0A31ਲ\x0A34ਵ\x0A37ਸ\x0A3Aਖ਼\x0A5Dਫ਼\x0A5Fੲੵઅઌઍ\x0A8Eએ\x0A92ઓ\x0AA9પ\x0AB1લ\x0AB4વ\x0ABAઽાૠૡଅ\x0B0Dଏ\x0B11ଓ\x0B29ପ\x0B31ଲ\x0B34ଶ\x0B3Aଽାଡ଼\x0B5Eୟୢஅ\x0B8Bஎ\x0B91ஒ\x0B96ங\x0B9Bஜ\x0B9Dஞ\x0BA0ண\x0BA5ந\x0BABமஶஷ\x0BBAఅ\x0C0Dఎ\x0C11ఒ\x0C29పఴవ\x0C3Aౠౢಅ\x0C8Dಎ\x0C91ಒ\x0CA9ಪ\x0CB4ವ\x0CBAೞ\x0CDFೠೢഅ\x0D0Dഎ\x0D11ഒഩപഺൠൢกฯะัาิเๆກ\x0E83ຄ\x0E85ງ\x0E89ຊ\x0E8Bຍ\x0E8Eດ\x0E98ນ\x0EA0ມ\x0EA4ລ\x0EA6ວ\x0EA8ສ\x0EACອຯະັາິຽ\x0EBEເ\x0EC5ཀ\x0F48ཉཪႠ\x10C6აჷᄀᄁᄂᄄᄅᄈᄉᄊᄋᄍᄎᄓᄼᄽᄾᄿᅀᅁᅌᅍᅎᅏᅐᅑᅔᅖᅙᅚᅟᅢᅣᅤᅥᅦᅧᅨᅩᅪᅭᅯᅲᅴᅵᅶᆞᆟᆨᆩᆫᆬᆮᆰᆷᆹᆺᆻᆼᇃᇫᇬᇰᇱᇹᇺḀẜẠỺἀ\x1F16Ἐ\x1F1Eἠ\x1F46Ὀ\x1F4Eὐ\x1F58Ὑ\x1F5AὛ\x1F5CὝ\x1F5EὟ\x1F7Eᾀ\x1FB5ᾶ᾽ι᾿ῂ\x1FC5ῆ῍ῐ\x1FD4ῖ\x1FDCῠ῭ῲ\x1FF5ῶ´Ω℧Kℬ℮ℯↀↃ〇〈〡〪ぁゕァ・ㄅㄭ一龦가\xD7A4"
      },
      {
        "_xmlW",
        "$%+,0:<?A[^_`{|}~\x007F¢«¬\x00AD®·¸»\x00BC¿ÀȡȢȴɐʮʰ˯̀͐͠ͰʹͶͺͻ΄·Έ\x038BΌ\x038DΎ\x03A2ΣϏϐϷЀ҇҈ӏӐӶӸӺԀԐԱ\x0557ՙ՚ա\x0588ֺֻ֑֢֣־ֿ׀ׁ׃ׅׄא\x05EBװ׳ءػـٖ٠٪ٮ۔ە\x06DD۞ۮ۰ۿܐܭܰ\x074Bހ\x07B2ँऄअऺ़ॎॐॕक़।०॰ঁ\x0984অ\x098Dএ\x0991ও\x09A9প\x09B1ল\x09B3শ\x09BA়ঽা\x09C5ে\x09C9োৎৗ\x09D8ড়\x09DEয়\x09E4০৻ਂਃਅ\x0A0Bਏ\x0A11ਓ\x0A29ਪ\x0A31ਲ\x0A34ਵ\x0A37ਸ\x0A3A਼\x0A3Dਾ\x0A43ੇ\x0A49ੋ\x0A4Eਖ਼\x0A5Dਫ਼\x0A5F੦ੵઁ\x0A84અઌઍ\x0A8Eએ\x0A92ઓ\x0AA9પ\x0AB1લ\x0AB4વ\x0ABA઼\x0AC6ે\x0ACAો\x0ACEૐ\x0AD1ૠૡ૦૰ଁ\x0B04ଅ\x0B0Dଏ\x0B11ଓ\x0B29ପ\x0B31ଲ\x0B34ଶ\x0B3A଼ୄେ\x0B49ୋ\x0B4Eୖ\x0B58ଡ଼\x0B5Eୟୢ୦ୱஂ\x0B84அ\x0B8Bஎ\x0B91ஒ\x0B96ங\x0B9Bஜ\x0B9Dஞ\x0BA0ண\x0BA5ந\x0BABமஶஷ\x0BBAா\x0BC3ெ\x0BC9ொ\x0BCEௗ\x0BD8௧௳ఁ\x0C04అ\x0C0Dఎ\x0C11ఒ\x0C29పఴవ\x0C3Aా\x0C45ె\x0C49ొ\x0C4Eౕ\x0C57ౠౢ౦\x0C70ಂ\x0C84ಅ\x0C8Dಎ\x0C91ಒ\x0CA9ಪ\x0CB4ವ\x0CBAಾ\x0CC5ೆ\x0CC9ೊ\x0CCEೕ\x0CD7ೞ\x0CDFೠೢ೦\x0CF0ം\x0D04അ\x0D0Dഎ\x0D11ഒഩപഺാൄെ\x0D49ൊൎൗ\x0D58ൠൢ൦\x0D70ං\x0D84අ\x0D97ක\x0DB2ඳ\x0DBCල\x0DBEව\x0DC7්\x0DCBා\x0DD5ූ\x0DD7ෘ\x0DE0ෲ෴ก\x0E3B฿๏๐๚ກ\x0E83ຄ\x0E85ງ\x0E89ຊ\x0E8Bຍ\x0E8Eດ\x0E98ນ\x0EA0ມ\x0EA4ລ\x0EA6ວ\x0EA8ສ\x0EACອ\x0EBAົ\x0EBEເ\x0EC5ໆ\x0EC7່\x0ECE໐\x0EDAໜໞༀ༄༓༺༾\x0F48ཉཫཱ྅྆ྌྐ\x0F98ྙ\x0FBD྾\x0FCD࿏࿐ကဢဣဨဩါာဳံ်၀၊ၐၚႠ\x10C6აჹᄀᅚᅟᆣᆨᇺሀሇለቇቈ\x1249ቊ\x124Eቐ\x1257ቘ\x1259ቚ\x125Eበኇኈ\x1289ኊ\x128Eነኯኰ\x12B1ኲ\x12B6ኸ\x12BFዀ\x12C1ዂ\x12C6ወዏዐ\x12D7ዘዯደጏጐ\x1311ጒ\x1316ጘጟጠፇፈ\x135B\x1369\x137DᎠᏵᐁ᙭ᙯᙷᚁ᚛ᚠ᛫ᛮᛱᜀ\x170Dᜎ\x1715ᜠ᜵ᝀ\x1754ᝠ\x176Dᝮ\x1771ᝲ\x1774ក។ៗ៘៛៝០\x17EA᠋\x180E᠐\x181Aᠠ\x1878ᢀᢪḀẜẠỺἀ\x1F16Ἐ\x1F1Eἠ\x1F46Ὀ\x1F4Eὐ\x1F58Ὑ\x1F5AὛ\x1F5CὝ\x1F5EὟ\x1F7Eᾀ\x1FB5ᾶ\x1FC5ῆ\x1FD4ῖ\x1FDC῝\x1FF0ῲ\x1FF5ῶ\x1FFF⁄⁅⁒⁓\x2070\x2072\x2074⁽ⁿ₍₠₲⃫⃐℀℻ℽ⅌\x2153ↄ←〈⌫⎴⎷⏏␀\x2427⑀\x244B\x2460\x24FF─☔☖☘☙♾⚀⚊✁✅✆✊✌✨✩❌❍❎❏❓❖❗❘❟❡❨\x2776➕➘➰➱➿⟐⟦⟰⦃⦙⧘⧜⧼⧾⬀⺀\x2E9A⺛\x2EF4⼀\x2FD6⿰\x2FFC〄〈〒〔〠〰〱〽〾\x3040ぁ\x3097゙゠ァ・ー\x3100ㄅㄭㄱ\x318F㆐ㆸㇰ㈝\x3220㉄\x3251㉼㉿㋌㋐\x32FF㌀㍷㍻㏞㏠㏿㐀\x4DB6一龦ꀀ\xA48D꒐\xA4C7가\xD7A4豈郞侮恵ﬀ\xFB07ﬓ\xFB18יִ\xFB37טּ\xFB3Dמּ\xFB3Fנּ\xFB42ףּ\xFB45צּ﮲ﯓ﴾ﵐ\xFD90ﶒ\xFDC8ﷰ﷽︀︐︠︤﹢﹣﹤\xFE67﹩﹪ﹰ\xFE75ﹶ\xFEFD＄％＋，０：＜？Ａ［＾＿｀｛｜｝～｟ｦ\xFFBFￂ\xFFC8ￊ\xFFD0ￒ\xFFD8ￚ\xFFDD￠\xFFE7￨\xFFEF￼\xFFFE"
      }
        };
        private static readonly RegexCharClass.LowerCaseMapping[] _lcTable = new RegexCharClass.LowerCaseMapping[94]
        {
      new RegexCharClass.LowerCaseMapping('A', 'Z', 1, 32),
      new RegexCharClass.LowerCaseMapping('À', 'Þ', 1, 32),
      new RegexCharClass.LowerCaseMapping('Ā', 'Į', 2, 0),
      new RegexCharClass.LowerCaseMapping('İ', 'İ', 0, 105),
      new RegexCharClass.LowerCaseMapping('Ĳ', 'Ķ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ĺ', 'Ň', 3, 0),
      new RegexCharClass.LowerCaseMapping('Ŋ', 'Ŷ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ÿ', 'Ÿ', 0,  byte.MaxValue),
      new RegexCharClass.LowerCaseMapping('Ź', 'Ž', 3, 0),
      new RegexCharClass.LowerCaseMapping('Ɓ', 'Ɓ', 0, 595),
      new RegexCharClass.LowerCaseMapping('Ƃ', 'Ƅ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ɔ', 'Ɔ', 0, 596),
      new RegexCharClass.LowerCaseMapping('Ƈ', 'Ƈ', 0, 392),
      new RegexCharClass.LowerCaseMapping('Ɖ', 'Ɗ', 1, 205),
      new RegexCharClass.LowerCaseMapping('Ƌ', 'Ƌ', 0, 396),
      new RegexCharClass.LowerCaseMapping('Ǝ', 'Ǝ', 0, 477),
      new RegexCharClass.LowerCaseMapping('Ə', 'Ə', 0, 601),
      new RegexCharClass.LowerCaseMapping('Ɛ', 'Ɛ', 0, 603),
      new RegexCharClass.LowerCaseMapping('Ƒ', 'Ƒ', 0, 402),
      new RegexCharClass.LowerCaseMapping('Ɠ', 'Ɠ', 0, 608),
      new RegexCharClass.LowerCaseMapping('Ɣ', 'Ɣ', 0, 611),
      new RegexCharClass.LowerCaseMapping('Ɩ', 'Ɩ', 0, 617),
      new RegexCharClass.LowerCaseMapping('Ɨ', 'Ɨ', 0, 616),
      new RegexCharClass.LowerCaseMapping('Ƙ', 'Ƙ', 0, 409),
      new RegexCharClass.LowerCaseMapping('Ɯ', 'Ɯ', 0, 623),
      new RegexCharClass.LowerCaseMapping('Ɲ', 'Ɲ', 0, 626),
      new RegexCharClass.LowerCaseMapping('Ɵ', 'Ɵ', 0, 629),
      new RegexCharClass.LowerCaseMapping('Ơ', 'Ƥ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ƨ', 'Ƨ', 0, 424),
      new RegexCharClass.LowerCaseMapping('Ʃ', 'Ʃ', 0, 643),
      new RegexCharClass.LowerCaseMapping('Ƭ', 'Ƭ', 0, 429),
      new RegexCharClass.LowerCaseMapping('Ʈ', 'Ʈ', 0, 648),
      new RegexCharClass.LowerCaseMapping('Ư', 'Ư', 0, 432),
      new RegexCharClass.LowerCaseMapping('Ʊ', 'Ʋ', 1, 217),
      new RegexCharClass.LowerCaseMapping('Ƴ', 'Ƶ', 3, 0),
      new RegexCharClass.LowerCaseMapping('Ʒ', 'Ʒ', 0, 658),
      new RegexCharClass.LowerCaseMapping('Ƹ', 'Ƹ', 0, 441),
      new RegexCharClass.LowerCaseMapping('Ƽ', 'Ƽ', 0, 445),
      new RegexCharClass.LowerCaseMapping('Ǆ', 'ǅ', 0, 454),
      new RegexCharClass.LowerCaseMapping('Ǉ', 'ǈ', 0, 457),
      new RegexCharClass.LowerCaseMapping('Ǌ', 'ǋ', 0, 460),
      new RegexCharClass.LowerCaseMapping('Ǎ', 'Ǜ', 3, 0),
      new RegexCharClass.LowerCaseMapping('Ǟ', 'Ǯ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ǳ', 'ǲ', 0, 499),
      new RegexCharClass.LowerCaseMapping('Ǵ', 'Ǵ', 0, 501),
      new RegexCharClass.LowerCaseMapping('Ǻ', 'Ȗ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ά', 'Ά', 0, 940),
      new RegexCharClass.LowerCaseMapping('Έ', 'Ί', 1, 37),
      new RegexCharClass.LowerCaseMapping('Ό', 'Ό', 0, 972),
      new RegexCharClass.LowerCaseMapping('Ύ', 'Ώ', 1, 63),
      new RegexCharClass.LowerCaseMapping('Α', 'Ϋ', 1, 32),
      new RegexCharClass.LowerCaseMapping('Ϣ', 'Ϯ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ё', 'Џ', 1, 80),
      new RegexCharClass.LowerCaseMapping('А', 'Я', 1, 32),
      new RegexCharClass.LowerCaseMapping('Ѡ', 'Ҁ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ґ', 'Ҿ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ӂ', 'Ӄ', 3, 0),
      new RegexCharClass.LowerCaseMapping('Ӈ', 'Ӈ', 0, 1224),
      new RegexCharClass.LowerCaseMapping('Ӌ', 'Ӌ', 0, 1228),
      new RegexCharClass.LowerCaseMapping('Ӑ', 'Ӫ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ӯ', 'Ӵ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ӹ', 'Ӹ', 0, 1273),
      new RegexCharClass.LowerCaseMapping('Ա', 'Ֆ', 1, 48),
      new RegexCharClass.LowerCaseMapping('Ⴀ', 'Ⴥ', 1, 48),
      new RegexCharClass.LowerCaseMapping('Ḁ', 'Ỹ', 2, 0),
      new RegexCharClass.LowerCaseMapping('Ἀ', 'Ἇ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ἐ', '\x1F1F', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ἠ', 'Ἧ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ἰ', 'Ἷ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ὀ', 'Ὅ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ὑ', 'Ὑ', 0, 8017),
      new RegexCharClass.LowerCaseMapping('Ὓ', 'Ὓ', 0, 8019),
      new RegexCharClass.LowerCaseMapping('Ὕ', 'Ὕ', 0, 8021),
      new RegexCharClass.LowerCaseMapping('Ὗ', 'Ὗ', 0, 8023),
      new RegexCharClass.LowerCaseMapping('Ὠ', 'Ὧ', 1, -8),
      new RegexCharClass.LowerCaseMapping('ᾈ', 'ᾏ', 1, -8),
      new RegexCharClass.LowerCaseMapping('ᾘ', 'ᾟ', 1, -8),
      new RegexCharClass.LowerCaseMapping('ᾨ', 'ᾯ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ᾰ', 'Ᾱ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ὰ', 'Ά', 1, -74),
      new RegexCharClass.LowerCaseMapping('ᾼ', 'ᾼ', 0, 8115),
      new RegexCharClass.LowerCaseMapping('Ὲ', 'Ή', 1, -86),
      new RegexCharClass.LowerCaseMapping('ῌ', 'ῌ', 0, 8131),
      new RegexCharClass.LowerCaseMapping('Ῐ', 'Ῑ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ὶ', 'Ί', 1, -100),
      new RegexCharClass.LowerCaseMapping('Ῠ', 'Ῡ', 1, -8),
      new RegexCharClass.LowerCaseMapping('Ὺ', 'Ύ', 1, -112),
      new RegexCharClass.LowerCaseMapping('Ῥ', 'Ῥ', 0, 8165),
      new RegexCharClass.LowerCaseMapping('Ὸ', 'Ό', 1,  sbyte.MinValue),
      new RegexCharClass.LowerCaseMapping('Ὼ', 'Ώ', 1, -126),
      new RegexCharClass.LowerCaseMapping('ῼ', 'ῼ', 0, 8179),
      new RegexCharClass.LowerCaseMapping('Ⅰ', 'Ⅿ', 1, 16),
      new RegexCharClass.LowerCaseMapping('Ⓐ', 'ⓐ', 1, 26),
      new RegexCharClass.LowerCaseMapping('Ａ', 'Ｚ', 1, 32)
        };
        internal static readonly char[] Hex = new char[16]
        {
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      'a',
      'b',
      'c',
      'd',
      'e',
      'f'
        };
        internal static readonly string[] Categories = new string[31]
        {
      "Lu",
      "Ll",
      "Lt",
      "Lm",
      "Lo",
      RegexCharClass.InternalRegexIgnoreCase,
      "Mn",
      "Mc",
      "Me",
      "Nd",
      "Nl",
      "No",
      "Zs",
      "Zl",
      "Zp",
      "Cc",
      "Cf",
      "Cs",
      "Co",
      "Pc",
      "Pd",
      "Ps",
      "Pe",
      "Pi",
      "Pf",
      "Po",
      "Sm",
      "Sc",
      "Sk",
      "So",
      "Cn"
        };
        private const int FLAGS = 0;
        private const int SETLENGTH = 1;
        private const int CATEGORYLENGTH = 2;
        private const int SETSTART = 3;
        private const char Nullchar = '\0';
        private const char Lastchar = '\xFFFF';
        private const char GroupChar = '\0';
        private const short SpaceConst = 100;
        private const short NotSpaceConst = -100;
        private const char ZeroWidthJoiner = '\x200D';
        private const char ZeroWidthNonJoiner = '\x200C';
        private const string ECMASpaceSet = "\t\x000E !";
        private const string NotECMASpaceSet = "\0\t\x000E !";
        private const string ECMAWordSet = "0:A[_`a{İı";
        private const string NotECMAWordSet = "\00:A[_`a{İı";
        private const string ECMADigitSet = "0:";
        private const string NotECMADigitSet = "\00:";
        internal const string ECMASpaceClass = "\0\x0004\0\t\x000E !";
        internal const string NotECMASpaceClass = "\x0001\x0004\0\t\x000E !";
        internal const string ECMAWordClass = "\0\n\00:A[_`a{İı";
        internal const string NotECMAWordClass = "\x0001\n\00:A[_`a{İı";
        internal const string ECMADigitClass = "\0\x0002\00:";
        internal const string NotECMADigitClass = "\x0001\x0002\00:";
        internal const string AnyClass = "\0\x0001\0\0";
        internal const string EmptyClass = "\0\0\0";
        private const int LowercaseSet = 0;
        private const int LowercaseAdd = 1;
        private const int LowercaseBor = 2;
        private const int LowercaseBad = 3;
        private List<RegexCharClass.SingleRange> _rangelist;
        private StringBuilder _categories;
        private bool _canonical;
        private bool _negate;
        private RegexCharClass _subtractor;
        private static readonly string Word;
        private static readonly string NotWord;
        internal static readonly string SpaceClass;
        internal static readonly string NotSpaceClass;
        internal static readonly string WordClass;
        internal static readonly string NotWordClass;
        internal static readonly string DigitClass;
        internal static readonly string NotDigitClass;
        private static Dictionary<string, string> _definedCategories;

        static RegexCharClass()
        {
            var dictionary = new Dictionary<string, string>(32);
            var chArray = new char[9];
            var stringBuilder = new StringBuilder(11);
            stringBuilder.Append(char.MinValue);
            chArray[0] = char.MinValue;
            chArray[1] = '\x000F';
            dictionary["Cc"] = chArray[1].ToString();
            chArray[2] = '\x0010';
            dictionary["Cf"] = chArray[2].ToString();
            chArray[3] = '\x001E';
            dictionary["Cn"] = chArray[3].ToString();
            chArray[4] = '\x0012';
            dictionary["Co"] = chArray[4].ToString();
            chArray[5] = '\x0011';
            dictionary["Cs"] = chArray[5].ToString();
            chArray[6] = char.MinValue;
            dictionary["C"] = new string(chArray, 0, 7);
            chArray[1] = '\x0002';
            dictionary["Ll"] = chArray[1].ToString();
            chArray[2] = '\x0004';
            dictionary["Lm"] = chArray[2].ToString();
            chArray[3] = '\x0005';
            dictionary["Lo"] = chArray[3].ToString();
            chArray[4] = '\x0003';
            dictionary["Lt"] = chArray[4].ToString();
            chArray[5] = '\x0001';
            dictionary["Lu"] = chArray[5].ToString();
            dictionary["L"] = new string(chArray, 0, 7);
            stringBuilder.Append(new string(chArray, 1, 5));
            dictionary[RegexCharClass.InternalRegexIgnoreCase] = string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}{4}", char.MinValue, chArray[1], chArray[4], chArray[5], chArray[6]);
            chArray[1] = '\a';
            dictionary["Mc"] = chArray[1].ToString();
            chArray[2] = '\b';
            dictionary["Me"] = chArray[2].ToString();
            chArray[3] = '\x0006';
            dictionary["Mn"] = chArray[3].ToString();
            chArray[4] = char.MinValue;
            dictionary["M"] = new string(chArray, 0, 5);
            chArray[1] = '\t';
            dictionary["Nd"] = chArray[1].ToString();
            chArray[2] = '\n';
            dictionary["Nl"] = chArray[2].ToString();
            chArray[3] = '\v';
            dictionary["No"] = chArray[3].ToString();
            dictionary["N"] = new string(chArray, 0, 5);
            stringBuilder.Append(chArray[1]);
            chArray[1] = '\x0013';
            dictionary["Pc"] = chArray[1].ToString();
            chArray[2] = '\x0014';
            dictionary["Pd"] = chArray[2].ToString();
            chArray[3] = '\x0016';
            dictionary["Pe"] = chArray[3].ToString();
            chArray[4] = '\x0019';
            dictionary["Po"] = chArray[4].ToString();
            chArray[5] = '\x0015';
            dictionary["Ps"] = chArray[5].ToString();
            chArray[6] = '\x0018';
            dictionary["Pf"] = chArray[6].ToString();
            chArray[7] = '\x0017';
            dictionary["Pi"] = chArray[7].ToString();
            chArray[8] = char.MinValue;
            dictionary["P"] = new string(chArray, 0, 9);
            stringBuilder.Append(chArray[1]);
            chArray[1] = '\x001B';
            dictionary["Sc"] = chArray[1].ToString();
            chArray[2] = '\x001C';
            dictionary["Sk"] = chArray[2].ToString();
            chArray[3] = '\x001A';
            dictionary["Sm"] = chArray[3].ToString();
            chArray[4] = '\x001D';
            dictionary["So"] = chArray[4].ToString();
            chArray[5] = char.MinValue;
            dictionary["S"] = new string(chArray, 0, 6);
            chArray[1] = '\r';
            dictionary["Zl"] = chArray[1].ToString();
            chArray[2] = '\x000E';
            dictionary["Zp"] = chArray[2].ToString();
            chArray[3] = '\f';
            dictionary["Zs"] = chArray[3].ToString();
            chArray[4] = char.MinValue;
            dictionary["Z"] = new string(chArray, 0, 5);
            stringBuilder.Append(char.MinValue);
            RegexCharClass.Word = stringBuilder.ToString();
            RegexCharClass.NotWord = RegexCharClass.NegateCategory(RegexCharClass.Word);
            RegexCharClass.SpaceClass = "\0\0\x0001" + RegexCharClass.Space;
            RegexCharClass.NotSpaceClass = "\x0001\0\x0001" + RegexCharClass.Space;
            RegexCharClass.WordClass = "\0\0" + (char)RegexCharClass.Word.Length + RegexCharClass.Word;
            RegexCharClass.NotWordClass = "\x0001\0" + (char)RegexCharClass.Word.Length + RegexCharClass.Word;
            RegexCharClass.DigitClass = "\0\0\x0001" + '\t';
            RegexCharClass.NotDigitClass = "\0\0\x0001" + '\xFFF7';
            RegexCharClass._definedCategories = dictionary;
        }

        internal RegexCharClass()
        {
            _rangelist = new List<RegexCharClass.SingleRange>(6);
            _canonical = true;
            _categories = new StringBuilder();
        }

        private RegexCharClass(bool negate, List<RegexCharClass.SingleRange> ranges, StringBuilder categories, RegexCharClass subtraction)
        {
            _rangelist = ranges;
            _categories = categories;
            _canonical = true;
            _negate = negate;
            _subtractor = subtraction;
        }

        internal bool CanMerge
        {
            get
            {
                if (!_negate)
                    return _subtractor == null;
                return false;
            }
        }

        internal bool Negate
        {
            set
            {
                _negate = value;
            }
        }

        internal void AddChar(char c)
        {
            AddRange(c, c);
        }

        internal void AddCharClass(RegexCharClass cc)
        {
            if (!cc._canonical)
                _canonical = false;
            else if (_canonical && RangeCount() > 0 && (cc.RangeCount() > 0 && cc.GetRangeAt(0)._first <= GetRangeAt(RangeCount() - 1)._last))
                _canonical = false;
            for (int i = 0; i < cc.RangeCount(); ++i)
                _rangelist.Add(cc.GetRangeAt(i));
            _categories.Append(cc._categories.ToString());
        }

        private void AddSet(string set)
        {
            if (_canonical && RangeCount() > 0 && (set.Length > 0 && set[0] <= GetRangeAt(RangeCount() - 1)._last))
                _canonical = false;
            int index = 0;
            while (index < set.Length - 1)
            {
                _rangelist.Add(new RegexCharClass.SingleRange(set[index], (char)(set[index + 1] - 1U)));
                index += 2;
            }
            if (index >= set.Length)
                return;
            _rangelist.Add(new RegexCharClass.SingleRange(set[index], char.MaxValue));
        }

        internal void AddSubtraction(RegexCharClass sub)
        {
            _subtractor = sub;
        }

        internal void AddRange(char first, char last)
        {
            _rangelist.Add(new RegexCharClass.SingleRange(first, last));
            if (!_canonical || _rangelist.Count <= 0 || first > _rangelist[_rangelist.Count - 1]._last)
                return;
            _canonical = false;
        }

        internal void AddCategoryFromName(string categoryName, bool invert, bool caseInsensitive, string pattern)
        {
            string str;
            RegexCharClass._definedCategories.TryGetValue(categoryName, out str);
            if (str != null && !categoryName.Equals(RegexCharClass.InternalRegexIgnoreCase))
            {
                string category = str;
                if (caseInsensitive && (categoryName.Equals("Ll") || categoryName.Equals("Lu") || categoryName.Equals("Lt")))
                    category = RegexCharClass._definedCategories[RegexCharClass.InternalRegexIgnoreCase];
                if (invert)
                    category = RegexCharClass.NegateCategory(category);
                _categories.Append(category);
            }
            else
                AddSet(RegexCharClass.SetFromProperty(categoryName, invert, pattern));
        }

        private void AddCategory(string category)
        {
            _categories.Append(category);
        }

        internal void AddLowercase(CultureInfo culture)
        {
            _canonical = false;
            int index = 0;
            for (int count = _rangelist.Count; index < count; ++index)
            {
                RegexCharClass.SingleRange singleRange = _rangelist[index];
                if (singleRange._first == singleRange._last)
                    singleRange._first = singleRange._last = char.ToLower(singleRange._first, culture);
                else
                    AddLowercaseRange(singleRange._first, singleRange._last, culture);
            }
        }

        private void AddLowercaseRange(char chMin, char chMax, CultureInfo culture)
        {
            int index1 = 0;
            int num = RegexCharClass._lcTable.Length;
            while (index1 < num)
            {
                int index2 = (index1 + num) / 2;
                if (RegexCharClass._lcTable[index2]._chMax < chMin)
                    index1 = index2 + 1;
                else
                    num = index2;
            }
            if (index1 >= RegexCharClass._lcTable.Length)
                return;
            RegexCharClass.LowerCaseMapping lowerCaseMapping;
            for (; index1 < RegexCharClass._lcTable.Length && (lowerCaseMapping = RegexCharClass._lcTable[index1])._chMin <= chMax; ++index1)
            {
                char first;
                if ((first = lowerCaseMapping._chMin) < chMin)
                    first = chMin;
                char last;
                if ((last = lowerCaseMapping._chMax) > chMax)
                    last = chMax;
                switch (lowerCaseMapping._lcOp)
                {
                    case 0:
                        first = (char)lowerCaseMapping._data;
                        last = (char)lowerCaseMapping._data;
                        break;
                    case 1:
                        first += (char)lowerCaseMapping._data;
                        last += (char)lowerCaseMapping._data;
                        break;
                    case 2:
                        first |= '\x0001';
                        last |= '\x0001';
                        break;
                    case 3:
                        first += (char)(first & 1U);
                        last += (char)(last & 1U);
                        break;
                }
                if (first < chMin || last > chMax)
                    AddRange(first, last);
            }
        }

        internal void AddWord(bool ecma, bool negate)
        {
            if (negate)
            {
                if (ecma)
                    AddSet("\00:A[_`a{İı");
                else
                    AddCategory(RegexCharClass.NotWord);
            }
            else if (ecma)
                AddSet("0:A[_`a{İı");
            else
                AddCategory(RegexCharClass.Word);
        }

        internal void AddSpace(bool ecma, bool negate)
        {
            if (negate)
            {
                if (ecma)
                    AddSet("\0\t\x000E !");
                else
                    AddCategory(RegexCharClass.NotSpace);
            }
            else if (ecma)
                AddSet("\t\x000E !");
            else
                AddCategory(RegexCharClass.Space);
        }

        internal void AddDigit(bool ecma, bool negate, string pattern)
        {
            if (ecma)
            {
                if (negate)
                    AddSet("\00:");
                else
                    AddSet("0:");
            }
            else
                AddCategoryFromName("Nd", negate, false, pattern);
        }

        internal static string ConvertOldStringsToClass(string set, string category)
        {
            var stringBuilder = new StringBuilder(set.Length + category.Length + 3);
            if (set.Length >= 2 && set[0] == 0 && set[1] == 0)
            {
                stringBuilder.Append('\x0001');
                stringBuilder.Append((char)(set.Length - 2));
                stringBuilder.Append((char)category.Length);
                stringBuilder.Append(set.Substring(2));
            }
            else
            {
                stringBuilder.Append(char.MinValue);
                stringBuilder.Append((char)set.Length);
                stringBuilder.Append((char)category.Length);
                stringBuilder.Append(set);
            }
            stringBuilder.Append(category);
            return stringBuilder.ToString();
        }

        internal static char SingletonChar(string set)
        {
            return set[3];
        }

        internal static bool IsMergeable(string charClass)
        {
            if (!RegexCharClass.IsNegated(charClass))
                return !RegexCharClass.IsSubtraction(charClass);
            return false;
        }

        internal static bool IsEmpty(string charClass)
        {
            return charClass[2] == 0 && charClass[0] == 0 && (charClass[1] == 0 && !RegexCharClass.IsSubtraction(charClass));
        }

        internal static bool IsSingleton(string set)
        {
            return set[0] == 0 && set[2] == 0 && (set[1] == 2 && !RegexCharClass.IsSubtraction(set)) && (set[3] == ushort.MaxValue || set[3] + 1 == set[4]);
        }

        internal static bool IsSingletonInverse(string set)
        {
            return set[0] == 1 && set[2] == 0 && (set[1] == 2 && !RegexCharClass.IsSubtraction(set)) && (set[3] == ushort.MaxValue || set[3] + 1 == set[4]);
        }

        private static bool IsSubtraction(string charClass)
        {
            return charClass.Length > 3 + charClass[1] + charClass[2];
        }

        internal static bool IsNegated(string set)
        {
            if (set != null)
                return set[0] == 1;
            return false;
        }

        internal static bool IsECMAWordChar(char ch)
        {
            return RegexCharClass.CharInClass(ch, "\0\n\00:A[_`a{İı");
        }

        internal static bool IsWordChar(char ch)
        {
            if (!RegexCharClass.CharInClass(ch, RegexCharClass.WordClass) && ch != 8205)
                return ch == 8204;
            return true;
        }

        internal static bool CharInClass(char ch, string set)
        {
            return RegexCharClass.CharInClassRecursive(ch, set, 0);
        }

        internal static bool CharInClassRecursive(char ch, string set, int start)
        {
            int mySetLength = set[start + 1];
            int myCategoryLength = set[start + 2];
            int start1 = start + 3 + mySetLength + myCategoryLength;
            bool flag1 = false;
            if (set.Length > start1)
                flag1 = RegexCharClass.CharInClassRecursive(ch, set, start1);
            bool flag2 = RegexCharClass.CharInClassInternal(ch, set, start, mySetLength, myCategoryLength);
            if (set[start] == 1)
                flag2 = !flag2;
            if (flag2)
                return !flag1;
            return false;
        }

        private static bool CharInClassInternal(char ch, string set, int start, int mySetLength, int myCategoryLength)
        {
            int num1 = start + 3;
            int num2 = num1 + mySetLength;
            while (num1 != num2)
            {
                int index = (num1 + num2) / 2;
                if (ch < set[index])
                    num2 = index;
                else
                    num1 = index + 1;
            }
            if ((num1 & 1) == (start & 1))
                return true;
            if (myCategoryLength == 0)
                return false;
            return RegexCharClass.CharInCategory(ch, set, start, mySetLength, myCategoryLength);
        }

        private static bool CharInCategory(char ch, string set, int start, int mySetLength, int myCategoryLength)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(ch);
            int i = start + 3 + mySetLength;
            int num1 = i + myCategoryLength;
            while (i < num1)
            {
                int num2 = (short)set[i];
                if (num2 == 0)
                {
                    if (RegexCharClass.CharInCategoryGroup(ch, unicodeCategory, set, ref i))
                        return true;
                }
                else if (num2 > 0)
                {
                    if (num2 == 100)
                    {
                        if (char.IsWhiteSpace(ch))
                            return true;
                        ++i;
                        continue;
                    }
                    int num3 = num2 - 1;
                    if (unicodeCategory == (UnicodeCategory)num3)
                        return true;
                }
                else
                {
                    if (num2 == -100)
                    {
                        if (!char.IsWhiteSpace(ch))
                            return true;
                        ++i;
                        continue;
                    }
                    int num3 = -1 - num2;
                    if (unicodeCategory != (UnicodeCategory)num3)
                        return true;
                }
                ++i;
            }
            return false;
        }

        private static bool CharInCategoryGroup(char ch, UnicodeCategory chcategory, string category, ref int i)
        {
            ++i;
            int num1 = (short)category[i];
            if (num1 > 0)
            {
                bool flag = false;
                for (; num1 != 0; num1 = (short)category[i])
                {
                    if (!flag)
                    {
                        int num2 = num1 - 1;
                        if (chcategory == (UnicodeCategory)num2)
                            flag = true;
                    }
                    ++i;
                }
                return flag;
            }
            bool flag1 = true;
            for (; num1 != 0; num1 = (short)category[i])
            {
                if (flag1)
                {
                    int num2 = -1 - num1;
                    if (chcategory == (UnicodeCategory)num2)
                        flag1 = false;
                }
                ++i;
            }
            return flag1;
        }

        private static string NegateCategory(string category)
        {
            if (category == null)
                return null;
            var stringBuilder = new StringBuilder(category.Length);
            for (int index = 0; index < category.Length; ++index)
            {
                var num = (short)category[index];
                stringBuilder.Append((char)-num);
            }
            return stringBuilder.ToString();
        }

        internal static RegexCharClass Parse(string charClass)
        {
            return RegexCharClass.ParseRecursive(charClass, 0);
        }

        private static RegexCharClass ParseRecursive(string charClass, int start)
        {
            int capacity = charClass[start + 1];
            int length = charClass[start + 2];
            int start1 = start + 3 + capacity + length;
            var ranges = new List<RegexCharClass.SingleRange>(capacity);
            int index1 = start + 3;
            int startIndex = index1 + capacity;
            while (index1 < startIndex)
            {
                char first = charClass[index1];
                int index2 = index1 + 1;
                char last = index2 >= startIndex ? char.MaxValue : (char)(charClass[index2] - 1U);
                index1 = index2 + 1;
                ranges.Add(new RegexCharClass.SingleRange(first, last));
            }
            RegexCharClass subtraction = null;
            if (charClass.Length > start1)
                subtraction = RegexCharClass.ParseRecursive(charClass, start1);
            return new RegexCharClass(charClass[start] == 1, ranges, new StringBuilder(charClass.Substring(startIndex, length)), subtraction);
        }

        private int RangeCount()
        {
            return _rangelist.Count;
        }

        internal string ToStringClass()
        {
            if (!_canonical)
                Canonicalize();
            int num1 = _rangelist.Count * 2;
            var stringBuilder = new StringBuilder(num1 + _categories.Length + 3);
            int num2 = !_negate ? 0 : 1;
            stringBuilder.Append((char)num2);
            stringBuilder.Append((char)num1);
            stringBuilder.Append((char)_categories.Length);
            for (int index = 0; index < _rangelist.Count; ++index)
            {
                RegexCharClass.SingleRange singleRange = _rangelist[index];
                stringBuilder.Append(singleRange._first);
                if (singleRange._last != ushort.MaxValue)
                    stringBuilder.Append((char)(singleRange._last + 1U));
            }
            stringBuilder[1] = (char)(stringBuilder.Length - 3);
            stringBuilder.Append(_categories);
            if (_subtractor != null)
                stringBuilder.Append(_subtractor.ToStringClass());
            return stringBuilder.ToString();
        }

        private RegexCharClass.SingleRange GetRangeAt(int i)
        {
            return _rangelist[i];
        }

        private void Canonicalize()
        {
            _canonical = true;
            _rangelist.Sort(0, _rangelist.Count, new RegexCharClass.SingleRangeComparer());
            if (_rangelist.Count <= 1)
                return;
            bool flag = false;
            int index1 = 1;
            int index2 = 0;
            while (true)
            {
                char last;
                for (last = _rangelist[index2]._last; index1 != _rangelist.Count && last != ushort.MaxValue; ++index1)
                {
                    RegexCharClass.SingleRange singleRange;
                    if ((singleRange = _rangelist[index1])._first <= last + 1)
                    {
                        if (last < singleRange._last)
                            last = singleRange._last;
                    }
                    else
                        goto label_9;
                }
                flag = true;
                label_9:
                _rangelist[index2]._last = last;
                ++index2;
                if (!flag)
                {
                    if (index2 < index1)
                        _rangelist[index2] = _rangelist[index1];
                    ++index1;
                }
                else
                    break;
            }
            _rangelist.RemoveRange(index2, _rangelist.Count - index2);
        }

        private static string SetFromProperty(string capname, bool invert, string pattern)
        {
            int num1 = 0;
            int num2 = RegexCharClass._propTable.GetLength(0);
            while (num1 != num2)
            {
                int index = (num1 + num2) / 2;
                int num3 = string.Compare(capname, RegexCharClass._propTable[index, 0], StringComparison.Ordinal);
                if (num3 < 0)
                    num2 = index;
                else if (num3 > 0)
                {
                    num1 = index + 1;
                }
                else
                {
                    string str = RegexCharClass._propTable[index, 1];
                    if (!invert)
                        return str;
                    if (str[0] == 0)
                        return str.Substring(1);
                    return 0.ToString() + str;
                }
            }
            throw new ArgumentException(Strings.GetString("MakeException", pattern, Strings.GetString("UnknownProperty", (object)capname)));
        }

        internal static string SetDescription(string set)
        {
            int startIndex1 = 3 + set[1] + set[2];
            var stringBuilder = new StringBuilder("[");
            int startIndex2 = 3;
            if (RegexCharClass.IsNegated(set))
                stringBuilder.Append('^');
            while (startIndex2 < 3 + set[1])
            {
                char ch1 = set[startIndex2];
                char ch2 = startIndex2 + 1 >= set.Length ? char.MaxValue : (char)(set[startIndex2 + 1] - 1U);
                stringBuilder.Append(RegexCharClass.CharDescription(ch1));
                if (ch2 != ch1)
                {
                    if (ch1 + 1 != ch2)
                        stringBuilder.Append('-');
                    stringBuilder.Append(RegexCharClass.CharDescription(ch2));
                }
                startIndex2 += 2;
            }
            for (; startIndex2 < 3 + set[1] + set[2]; ++startIndex2)
            {
                char ch = set[startIndex2];
                if (ch == 0)
                {
                    bool flag = false;
                    int num = set.IndexOf(char.MinValue, startIndex2 + 1);
                    string str = set.Substring(startIndex2, num - startIndex2 + 1);
                    IDictionaryEnumerator enumerator = RegexCharClass._definedCategories.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        if (str.Equals(enumerator.Value))
                        {
                            if ((short)set[startIndex2 + 1] > 0)
                                stringBuilder.Append("\\p{" + enumerator.Key + "}");
                            else
                                stringBuilder.Append("\\P{" + enumerator.Key + "}");
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        if (str.Equals(RegexCharClass.Word))
                            stringBuilder.Append("\\w");
                        else if (str.Equals(RegexCharClass.NotWord))
                            stringBuilder.Append("\\W");
                    }
                    startIndex2 = num;
                }
                else
                    stringBuilder.Append(RegexCharClass.CategoryDescription(ch));
            }
            if (set.Length > startIndex1)
            {
                stringBuilder.Append('-');
                stringBuilder.Append(RegexCharClass.SetDescription(set.Substring(startIndex1)));
            }
            stringBuilder.Append(']');
            return stringBuilder.ToString();
        }

        internal static string CharDescription(char ch)
        {
            var stringBuilder = new StringBuilder();
            if (ch == 92)
                return "\\\\";
            if (ch >= 32 && ch <= 126)
            {
                stringBuilder.Append(ch);
                return stringBuilder.ToString();
            }
            int num;
            if (ch < 256)
            {
                stringBuilder.Append("\\x");
                num = 8;
            }
            else
            {
                stringBuilder.Append("\\u");
                num = 16;
            }
            while (num > 0)
            {
                num -= 4;
                stringBuilder.Append(RegexCharClass.Hex[ch >> num & 15]);
            }
            return stringBuilder.ToString();
        }

        private static string CategoryDescription(char ch)
        {
            if (ch == 100)
                return "\\s";
            if ((short)ch == -100)
                return "\\S";
            if ((short)ch < 0)
                return "\\P{" + RegexCharClass.Categories[-(short)ch - 1] + "}";
            return "\\p{" + RegexCharClass.Categories[ch - 1] + "}";
        }

        private struct LowerCaseMapping
        {
            internal char _chMin;
            internal char _chMax;
            internal int _lcOp;
            internal int _data;

            internal LowerCaseMapping(char chMin, char chMax, int lcOp, int data)
            {
                _chMin = chMin;
                _chMax = chMax;
                _lcOp = lcOp;
                _data = data;
            }
        }

        private sealed class SingleRangeComparer : IComparer<RegexCharClass.SingleRange>
        {
            public int Compare(RegexCharClass.SingleRange x, RegexCharClass.SingleRange y)
            {
                if (x._first < y._first)
                    return -1;
                return x._first <= y._first ? 0 : 1;
            }
        }

        private sealed class SingleRange
        {
            internal char _first;
            internal char _last;

            internal SingleRange(char first, char last)
            {
                _first = first;
                _last = last;
            }
        }
    }
}
