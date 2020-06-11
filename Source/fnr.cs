using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FindAndReplaceTool
{
    class Fnr
    {
        static int Main(string[] args)
        {
            #region Start watch.
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            #endregion

            #region Top version information.
            // Get assembly file version.
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string[] version = fvi.FileVersion.Split("."); // Split by MAJOR | MINOR | PATCH | INTERNAL.

            // Calculate line width based on console width.
            string spacingFullwidth = "";
            for (int i = 0; i < Math.Floor(Convert.ToDecimal(Console.WindowWidth - 1)); i++)
            {
                spacingFullwidth += " ";
            }

            string topVersionString = String.Format("  Find And Replace Tool ~ Version: {0}.{1}.{2} (by KDb)  ", version[0], version[1], version[2]);
            string spacingRemainder = spacingFullwidth.Substring(0, spacingFullwidth.Length - topVersionString.Length);

            DrawLine();
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(spacingFullwidth);
            Console.WriteLine(topVersionString + spacingRemainder);
            Console.WriteLine(spacingFullwidth);
            Console.ResetColor();
            DrawLine();
            #endregion

            #region Options.

            #region Path.
            var pathOption = new Option<FileInfo>(
                    aliases: new string[] { "--path", "-p" },
                    description: "Absolute or relative valid text file path."
                )
            {
                Required = true
            }.ExistingOnly();
            #endregion

            #region Find.
            var findOption = new Option<string>(
                    aliases: new string[] { "--find", "-f" },
                    description: "Text value to be found."
                )
            {
                Required = true
            };
            #endregion

            #region Replace.
            var replaceOption = new Option<string>(
                    aliases: new string[] { "--replace", "-r" },
                    description: "Text replacement value (always case sensitive)."
                )
            {
                Required = true
            };
            #endregion

            #region Key.
            var keyOption = new Option<string>(
                    aliases: new string[] { "--key", "-k" },
                    description: "Only look for matches in value(s) paired with given key."
                );
            #endregion

            #region Section.
            var sectionOption = new Option<string>(
                    aliases: new string[] { "--section", "-s" },
                    description: "Only look for matches within given ini section."
                );
            #endregion

            #region Between [Start].
            var startOption = new Option<string>(
                    aliases: new string[] { "--start"/*, "--start", "--from"*/ },
                    description: "From where in file to start looking for matches."
                );
            #endregion

            #region Between [End].
            var endOption = new Option<string>(
                    aliases: new string[] { "--end"/*, "--end", "--upto"*/ },
                    description: "Up to where in file to end looking for matches."
                );
            #endregion

            #endregion

            #region Flags.

            #region Match case.
            var matchcaseOption = new Option<bool>(
                    aliases: new string[] { "--matchcase", "-c" },
                    description: "Enable case sensitive matching."
                );
            matchcaseOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            matchcaseOption.Argument.SetDefaultValue(false);
            #endregion

            #region Match word.
            var matchwordOption = new Option<bool>(
                    aliases: new string[] { "--matchword", "-w" },
                    description: "Enable precise word matching."
                );
            matchwordOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            matchwordOption.Argument.SetDefaultValue(false);
            #endregion

            #region Ignore comments.
            var ignorecommentsOption = new Option<bool>(
                    aliases: new string[] { "--ignorecomments", "-i" },
                    description: "Ignore comments."
                );
            ignorecommentsOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            ignorecommentsOption.Argument.SetDefaultValue(false);
            #endregion

            #region Allow duplication.
            var allowduplicationOption = new Option<bool>(
                    aliases: new string[] { "--allowduplication", "-d" },
                    description: "Find and replace even when text to be found is equal to or part of the replacement text."
                );
            allowduplicationOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            allowduplicationOption.Argument.SetDefaultValue(false);
            #endregion

            #region RegEx.
            var regexOption = new Option<bool>(
                    aliases: new string[] { "--regex" },
                    description: "Enable use of regular expressions in --find, --key, --section, --start & --end."
                );
            regexOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            regexOption.Argument.SetDefaultValue(false);
            #endregion

            #region Test.
            var testOption = new Option<bool>(
                    aliases: new string[] { "--test" },
                    description: "Preview matches without writing to file."
                );
            testOption.Argument.Arity = ArgumentArity.ZeroOrOne;
            testOption.Argument.SetDefaultValue(false);
            #endregion

            #endregion

            #region Root Command.

            #region Initialization.
            var rootCommand = new RootCommand
            {
                pathOption,
                findOption,
                replaceOption,
                keyOption,
                sectionOption,
                startOption,
                endOption,
                matchcaseOption,
                matchwordOption,
                ignorecommentsOption,
                allowduplicationOption,
                regexOption,
                testOption
            };
            #endregion

            #region Text defaults.
            rootCommand.Description = "A stand-alone command line based Find & Replace tool, optimized in order to replace text values in plain and/or configuration based text documents.";
            rootCommand.Name = "(.\\)fnr(.exe)";

            System.CommandLine.Help.DefaultHelpText.Usage.Title = "Usage example (anything between parentheses is optional):";
            System.CommandLine.Help.DefaultHelpText.Usage.Options = "--path \".\\example.ini\" --find \"disabled\" --replace \"enabled\" (--key \"example_key\") (--section \"Example Section\") (--start \"from here in text\") (--end \"up to here in text\") (--matchcase) (--matchword) (--ignorecomments) (--allowduplication) (--regex) (--test)";
            #endregion

            #region Custom validations.
            pathOption.AddValidator(path => {
                string pathOptionValue = path.GetValueOrDefault().ToString();

                if (File.Exists(pathOptionValue))
                    if(DetectEncoding(pathOptionValue) == null)
                        if (IsBinary(pathOptionValue))
                        {
                            // Binary file detected.
                            return "Binary file detected: " + Path.GetFileName(pathOptionValue);
                        }
                // If the file doesn't exist, it'll be caught by the ExistingOnly() check.

                return null; // No error.
            });
            #endregion

            #region Handler.

            // The parameters of the handler method are matched according to the names of the options.
            rootCommand.Handler = CommandHandler.Create<FileInfo, string, string, string, string, string, string, bool, bool, bool, bool, bool, bool>((path, find, replace, key, section, start, end, matchcase, matchword, ignorecomments, allowduplication, regex, test) =>
            {
                #region Provided options summary.

                #region Show a preventive warning if find & replace are exactly the same.
                string tmpFind = (matchcase) ? find : find.ToLower();
                string tmpReplace = (matchcase) ? replace : replace.ToLower();
                if (tmpFind == tmpReplace & !allowduplication)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("[WARNING] The --find and --replace value cancel each other out.");
                    Console.WriteLine("          Use the --matchcase and/or --allowduplication flag in order to match any results.");
                    Console.ResetColor();
                }
                #endregion

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Options]");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("{0,-14} {1,-0}", "- Path", $"'{path.FullName.Replace(@"C:\Apps\Find And Replace Tool (KDb)\Build\Self-Contained", @"C:\Apps\FNR Tool")}'"); // [DevNote] Replacement for screenshot taking purposes.
                /*Console.WriteLine("{0,-14} {1,-0}", "", "-> Detected encoding: '" + DetectEncoding(path.FullName).EncodingName + "'");*/
                Console.WriteLine("{0,-14} {1,-0}", "- Find", $"'{find}'");
                Console.WriteLine("{0,-14} {1,-0}", "- Replace", $"'{replace}'");
                if (!String.IsNullOrEmpty(section))
                    Console.WriteLine("{0,-14} {1,-0}", "- Section", "[" + $"{section}" + "]");
                if (!String.IsNullOrEmpty(key))
                    Console.WriteLine("{0,-14} {1,-0}", "- Key", $"'{key}'");
                if (!String.IsNullOrEmpty(start) | !String.IsNullOrEmpty(end))
                {
                    string inbetween = "";

                    if (!String.IsNullOrEmpty(start))
                        inbetween += $"'{start}'";
                    else
                        inbetween += "(beginning of file)";
                    inbetween += " & ";
                    if (!String.IsNullOrEmpty(end))
                        inbetween += $"'{end}'";
                    else
                        inbetween += "(end of file)";

                    Console.WriteLine("{0,-14} {1,-0}", "- In between", inbetween);
                }

                if (Convert.ToInt32(matchcase)
                    + Convert.ToInt32(matchword)
                    + Convert.ToInt32(ignorecomments)
                    + Convert.ToInt32(allowduplication)
                    + Convert.ToInt32(regex)
                    + Convert.ToInt32(test) == 1)
                {
                    DrawLine();
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("[Flag]");
                }
                if (Convert.ToInt32(matchcase)
                    + Convert.ToInt32(matchword)
                    + Convert.ToInt32(ignorecomments)
                    + Convert.ToInt32(allowduplication)
                    + Convert.ToInt32(regex)
                    + Convert.ToInt32(test) > 1)
                {
                    DrawLine();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("[Flags]");
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                if (matchcase)
                    Console.WriteLine("- Case sensitive matching enabled.");
                if (matchword)
                    Console.WriteLine("- Precise word matching enabled.");
                if (ignorecomments)
                    Console.WriteLine("- Comments ignored.");
                if (allowduplication)
                    Console.WriteLine("- Duplication allowed.");
                if (regex)
                    Console.WriteLine("- Regular expressions enabled.");
                if (test)
                    Console.WriteLine("- Preview mode enabled.");

                Console.ResetColor();

                DrawLine();
                #endregion

                #region Program logic.

                #region Read file.
                // Read the provided file.
                string sourceText = File.ReadAllText(path.FullName);
                #endregion

                #region Prerequisites.
                // Temporary set replacedText = sourceText.
                // (Will be overwritten if a match based on provided options is found.)
                string replacedText = sourceText;

                // Check if provided options should be escaped in order not to be interpretated as regular expression.
                if (!regex)
                {
                    find = (!String.IsNullOrEmpty(find)) ? Regex.Escape(find) : find;
                    key = (!String.IsNullOrEmpty(key)) ? Regex.Escape(key) : key;
                    section = (!String.IsNullOrEmpty(section)) ? Regex.Escape(section) : section;
                    start = (!String.IsNullOrEmpty(start)) ? Regex.Escape(start) : start;
                    end = (!String.IsNullOrEmpty(end)) ? Regex.Escape(end) : end;
                }
                #endregion

                #region Regular Expression buildup.

                string rgxPattern;

                #region Explanatory breakdown (comment).
                /*
                -----------------------------------------------------------------------------------
                Enhanced RegEx - Version v5d - All-encompassing.
                -----------------------------------------------------------------------------------
                (?<=(?<between_start>START)[.\s\S]*)
                (?<=(?<section_lbchar>\[)(?<section>\bSECTION\b)(?<section_rbchar>\])[.\s\S][^\[]*)
                (?((?#exclude_replace_value)REPLACE)
	                $donothing^
	                |
                    (?<!(?#exclude_trimmed_replace_value)NOTPRECEDEDBY)
	                (?<=(?<key_indent>\s*)(?<key>\bKEY\b)(?<in_between_key_value>.*))
                        (?<!(?#exclude_comment)^[#|;].*)
                        (?<=(?<context_left>.*))(?<value>\bFIND\b)(?=(?<context_right>.*))
                    (?=(?<ending>.*))
                )
                (?=[.\s\S]*(?<between_end>END))
                -----------------------------------------------------------------------------------
                */

                #region Online Regex Example (v5d) (Regex Storm).
                // http://regexstorm.net/tester?p=%28%3fmi%29%28%3f%3c%3d%28%3f%3cbetween_start%3elorem+ipsum%29%5b.%5cs%5cS%5d*%29%28%3f%3c%3d%28%3f%3csection_lbchar%3e%5c%5b%29%28%3f%3csection%3e%5cbCustom+Section%5cb%29%28%3f%3csection_rbchar%3e%5c%5d%29%5b.%5cs%5cS%5d%5b%5e%5c%5b%5d*%29%28%3f%28%28%3f%23exclude_replace_value%29SuperCool%29%24donothing%5e%7c%28%3f%3c!%28%3f%23exclude_trimmed_replace_value%29Super%29%28%3f%3c!%28%3f%23exclude_comment%29%5e%5b%23%7c%3b%5d.*%29%28%3f%3c%3d%28%3f%3ckey_indent%3e%5cs*%29%28%3f%3ckey%3e%5cbmenu_layout%5cb%29%28%3f%3cin_between_key_value%3e.*%29%29%28%3f%3c%3d%28%3f%3ccontext_left%3e.*%29%29%28%3f%3cvalue%3eCool%29%28%3f%3d%28%3f%3ccontext_right%3e.*%29%29%28%3f%3d%28%3f%3cending%3e.*%29%29%29%28%3f%3d%5b.%5cs%5cS%5d*%28%3f%3cbetween_end%3eretroarch%29%29&i=%28...%29%0d%0aThe+purpose+of+lorem+ipsum+is+to+create+a+natural+looking+block+of+text+%28sentence%2c+paragraph%2c+page%2c+etc.%29+that+doesn%27t+distract+from+the+layout.+%28...%29%0d%0a%0d%0a%5bCustom+Section%5d%0d%0akey%3dvalue%0d%0atruely_great_setting+%3d+Quite+a+setting.%0d%0adummy_setting_v%3denabled%0d%0adummy_setting_w+%3d+Enabled%0d%0adummy_setting_x+%3d+enabled%0d%0adummy_setting_y+%3d+++disabled%0d%0adummy_setting_z+++enabled%0d%0a%23menu_layout%09%3d+Cool+Theme%0d%0amenu_layout%09%09%3a+Cool+Theme+Cool+Theme+Theme+Cool%0d%0amenu_layout%09%09%3d+CoolSuper+Theme%0d%0a%09menu_layout%09%09%3d+SuperCool+Theme%0d%0amenu_layout%09%09%3d+Cool+Theme+-+Dark+Mode%0d%0amenu_layout%09%09%3d+SuperCool+Theme+-+Dark+Mode+Cool+Theme%0d%0ano_menu_layout%09%3d+SuperCool+Theme%0d%0amenu_layout%09%09+++Cool+Theme%0d%0asettings_show_audio+%3d+true%0d%0a%0d%0a%5bRetroArch%5d%0d%0asettings_show_audio+%3d+true%0d%0a%28...%29%0d%0a%0d%0a%5bRetroArch+Core+Options%5d%0d%0agambatte_gb_colorization+%3d+auto%0d%0a%0d%0a%28...%29
                #endregion

                #endregion

                #region Step 1) Value to be found/matched.
                // Value to be matched. /***[DevNote] str.Replace = case sensitive.***/
                if (!matchword)
                    rgxPattern = @"(?<=(?<context_left>.*))(?<value>FIND)(?=(?<context_right>.*))".Replace("FIND", find);
                else
                    rgxPattern = @"(?<=(?<context_left>.*))(?<value>\bFIND\b)(?=(?<context_right>.*))".Replace("FIND", find);
                #endregion

                #region Step 2) Paired with a key?
                if (!String.IsNullOrEmpty(key))  // If a key is provided, wrap around:
                    rgxPattern = @"(?<=(?<key_indent>\s*)(?<key>\bKEY\b)(?<in_between_key_value>.*))".Replace("KEY", key)   // If preceded by key. Everything on current line that comes before value to be matched.
                                 + rgxPattern +
                                 @"(?=(?<ending>.*))"; // Everything on current line that comes after key=value (f.e. whitespace, ', ", ...).
                #endregion

                #region Step 3) Ignore comments?
                if (ignorecomments)
                    rgxPattern = @"(?<!(?#exclude_comment)^[#|;].*)"   // If line starts with a comment character.
                          + rgxPattern;
                #endregion

                #region Step 4) Avoid duplication?
                if (!allowduplication)  // If the --allowduplication flag isn't provided, wrap around:
                {
                    #region In-depth (comment).
                    /*
                    [DevNote] This is a enhanced, well thought-through 2 step process:
                    ------------------------------------------------------------------
                    
                    1) SHOULD NOT BE FOLLOWED BY
                       -------------------------
                       F.e. --find 'Cool Theme' --replace 'Cool Theme - Lite Mode'
                       ==> find value should not be followed by ' - Lite Mode' in order to avoid duplication.
                           [DevNote] 2 ways (at least the ones I came up with, could be more) of checking this using regular expressions:
                           Option 1) (?(IFREPLACEVALUEMATCHED)matchnothing|elsekeepontryingtomatchFIND) => Originally went with this one before I noticed
                                                                                                           it didn't check what was in front.
                                                                                                           Doesn't really need any additional steps
                                                                                                           in order to determine a possible TrimFindValueAtStartOfReplaceValue remainder,
                                                                                                           so I decided to keep using this option.
                           Option 2) FIND(?<!NOTFOLLOWEDBYREMAINDER) => not followed by alternative     => Would need additional steps
                                                                                                           in order to determine a possible TrimFindValueAtStartOfReplaceValue remainder.
                       In other words:
                       ---------------
                       If the replace value is matched:
                       Stop matching, else continue matching find value.
                       [DevNote] This regex check is f.e. able to stop --find "Cool Theme" --replace "Cool Theme - Dark Mode" from matching,
                                 but would NOT stop --find "cool" --replace "supercool" from matching (hence option B).
                    
                    2) SHOULD NOT BE PRECEDED BY
                       -------------------------
                       F.e. --find 'cool' --replace 'supercool'
                       ==> find value should not be preceded by 'super' in order to avoid duplication.
                           [DevNote]  I went with regular expression: (?<!NOTPRECEDEDBY),
                                      requiring the following additional substeps in order to determine a possible TrimFindValueAtEndOfReplaceValue remainder:
                           Substep 1) notPrecededBy = TrimEnd(replace, find); => F.e. Remove 'cool' at end from 'supercool' => 'super'  (trimmed remainder result)
                                                                              => F.e. Remove 'cool' at end from 'cooler'    => 'cooler' (replace value didn't change == notPrecededBy).
                           Substep 2) if (notPrecededBy == replace)
                                          notPrecededBy = "$^nevermatch";     => If values are the same
                                                                                 (and thus nothing is removed at end,
                                                                                  meaning the find value isn't part of the right end of replace value),
                                                                                 set to a regex that would never match
                                                                                 in order to null out the (?<!NOTPRECEDEDBY) regex check.
                                                                                 F.e. not preceded by $^nevermatch
                                                                                      => would never match.  
                       In other words:
                       ---------------
                       If the find value is preceded by the TrimEnd result (which could be a nevermatch value depending on results in substeps):
                       Stop matching, else continue matching find value.
                       [DevNote] This regex check is f.e. able to stop --find "cool" --replace "supercool" from matching,
                                 but would NOT stop --find "Cool Theme" --replace "Cool Theme - Dark Mode" from matching (hence option A).
                    */
                    #endregion

                    #region Determine 'not preceded by' value.
                    // notPrecededBy should end up being either a trimmed result or a never matching regular expression in order to null out the regex check.
                    string notPrecededBy = TrimEnd(replace, find);
                    if (notPrecededBy == replace)
                        notPrecededBy = "$^nevermatch";
                    #endregion

                    rgxPattern = @"(?((?#exclude_replace_value)REPLACE)$donothing^|(?<!(?#exclude_trimmed_replace_value)NOTPRECEDEDBY)"
                                .Replace("REPLACE", replace)
                                .Replace("NOTPRECEDEDBY", notPrecededBy)
                          + rgxPattern +
                          @")"; // Close ((?Condition)yes|no).
                }
                #endregion

                #region Step 5) Contained within a section?
                if (!String.IsNullOrEmpty(section))  // If a section is provided, wrap around:
                    rgxPattern = @"(?<=(?<section_lbchar>\[)(?<section>\bSECTION\b)(?<section_rbchar>\])[.\s\S][^\[]*)".Replace("SECTION", section)   // If preceded by section.
                          + rgxPattern;
                #endregion

                #region Step 6) Find between.
                // A) Inbetween [start]:
                if (!String.IsNullOrEmpty(start))
                    rgxPattern = @"(?<=(?<between_start>START)[.\s\S]*)".Replace("START", start)   // If preceded by provided string.
                          + rgxPattern;
                else
                    rgxPattern = @"(?<=(?<between_start>START)[.\s\S]*)".Replace("START", "^")   // Else from ^ = beginning of text file.
                          + rgxPattern;
                // B) & [end]:
                if (!String.IsNullOrEmpty(end))
                    rgxPattern +=
                          @"(?=[.\s\S]*(?<between_end>END))".Replace("END", end);   // If followed by provided string.
                else
                    rgxPattern +=
                          @"(?=[.\s\S]*(?<between_end>END))".Replace("END", "$");   // Else up to $ = end of text file.
                #endregion

                #region Step 7) Add regex options.
                // RegexOptions.Multiline
                string regexOptions = "m";
                // RegexOptions.IgnoreCase
                if (!matchcase)
                    regexOptions += "i";

                rgxPattern = "(?" + regexOptions + ")"
                          + rgxPattern;
                #endregion

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Generated Regular Expression]");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(rgxPattern);
                Console.ResetColor();
                DrawLine();

                #endregion

                #region Match.
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Matches]");

                int matchCount = 0;

                /*RegexOptions options = new RegexOptions();
                if(!matchcase)
                    options = RegexOptions.IgnoreCase | RegexOptions.Multiline;
                else
                    options = RegexOptions.Multiline;*/

                replacedText = Regex.Replace(sourceText, rgxPattern, (match) =>
                {
                    matchCount++;

                    int line = LineFromPos(sourceText, match.Index);

                    // Show match:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("{0,-27} ", "-> Match found (line " + line + "): ");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_left"].Value.TrimStart());
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("{0,-0}", match.Value);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                    Console.WriteLine();
                    // Show replacement:
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("{0, 27} ", "(replaced) =>");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_left"].Value.TrimStart());
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("{0,-0}", replace);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.ResetColor();

                    return match.Result(replace);
                }/*, options*/);

                //if(matchCount != 0) DrawLine();
                //Console.WriteLine();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("=> " + matchCount.ToString() + (matchCount != 1 ? " matches" : " match") + " found" + (matchCount != 0 ? " and replaced with '" + replace + "'." : "."));
                if (test & matchCount != 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("   (Preview mode enabled = no file overwrite.)");
                }
                Console.ResetColor();
                #endregion

                #region Write file?
                if(!test)
                    File.WriteAllText(path.FullName, replacedText, DetectEncoding(path.FullName));

                DrawLine();
                #endregion

                #region Stop watch.
                watch.Stop();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Execution Time]");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"= {watch.ElapsedMilliseconds} milliseconds.");
                #endregion

                #endregion
            });

            #endregion

            #region Parse & Invoke.
            // Parse the incoming args and invoke the handler.
            return rootCommand.InvokeAsync(args).Result;
            #endregion

            #endregion
        }

        #region Private Methods.

        /// <summary>
        /// Checks whether a given file is binary or not.
        /// </summary>
        /// <param name="path">Absolute or relative path to file.</param>
        private static bool IsBinary(string path)
        {
            Stream objStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            bool bFlag = false;

            // Iterate through stream & check ASCII value of each byte.
            for (int nPosition = 0; nPosition < objStream.Length; nPosition++)
            {
                int a = objStream.ReadByte();

                if (!(a >= 0 && a <= 127))
                {
                    // [DevNote] Encoding checked before, but keeping it here for reference.
                    Encoding encoding = DetectEncoding(path);
                    if (encoding != null)
                        bFlag = false;  // Text file (with different encoding).
                    else
                        bFlag = true;   // Binary file.

                    break;
                }
                else if (objStream.Position == (objStream.Length))
                {
                    bFlag = false;      // Text file.
                }
            }
            objStream.Dispose();

            return bFlag;
        }

        /// <summary>
        /// Determines a text file's encoding using a port of Mozilla Universal Charset Detector.
        /// Defaults to null when encoding detection fails.
        /// </summary>
        /// <param name="path">Absolute or relative path to file.</param>
        /// <returns>The detected encoding or null.</returns>
        private static Encoding DetectEncoding(string path)
        {
            Encoding encoding;
            using (FileStream fs = File.OpenRead(path))
            {
                Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                cdet.Feed(fs);
                cdet.DataEnd();
                if (cdet.Charset != null)
                {
                    /*Console.WriteLine("Charset: {0}, confidence: {1}",
                         cdet.Charset, cdet.Confidence);*/
                    encoding = Encoding.GetEncoding(cdet.Charset);

                    return encoding;
                }
                else
                {
                    return null;
                }
            }
        }

        private static string TrimEnd(string input, string suffixToRemove)
        {
            if (input != null && suffixToRemove != null
              && input.EndsWith(suffixToRemove))
            {
                return input.Substring(0, input.Length - suffixToRemove.Length);
            }
            else return input;
        }

        /// <summary>
        /// Gets the line number based on a given index position.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="indexPosition">Index position.</param>
        private static int LineFromPos(string input, int indexPosition)
        {
            int lineNumber = 1;
            for (int i = 0; i < indexPosition; i++)
            {
                if (input[i] == '\n') lineNumber++;
            }
            return lineNumber;
        }

        /// <summary>
        /// Draws a log line.
        /// </summary>
        /// <param name="lineCharacter">Character to be used for line drawing. Defaults to '-'.</param>
        private static void DrawLine(char lineCharacter = '-')
        {
            // Store current console colors.
            ConsoleColor currForegroundColor = Console.ForegroundColor;
            ConsoleColor currBackgroundColor = Console.BackgroundColor;

            // Calculate line width based on console width.
            string lineFullwidth = "";
            for (int i = 0; i < Math.Floor(Convert.ToDecimal(Console.WindowWidth-1)); i++)
            {
                lineFullwidth += lineCharacter;
            }

            // Log without indentation.
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(lineFullwidth);

            // Reset to stored console colors.
            Console.ForegroundColor = currForegroundColor;
            Console.BackgroundColor = currBackgroundColor;
        }

        #endregion
    }
}
