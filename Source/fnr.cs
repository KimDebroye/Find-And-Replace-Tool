using ICSharpCode.WpfDesign.XamlDom;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace FindAndReplaceTool
{
    public static class Fnr
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

            #region XPath.
            var xpathOption = new Option<string>(
                    aliases: new string[] { "--xpath", "-x" },
                    description: "XML path expression."
                )
            {
                IsHidden = true
            };
            #endregion

            #region JPath.
            var jpathOption = new Option<string>(
                    aliases: new string[] { "--jpath", "-j" },
                    description: "JSON path expression."
                )
            {
                IsHidden = true
            };
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
                xpathOption,
                jpathOption,
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
            rootCommand.Handler = CommandHandler.Create<FileInfo, string, string, string, string, string, string, string, string, bool, bool, bool, bool, bool, bool>((path, find, replace, key, section, start, end, xpath, jpath, matchcase, matchword, ignorecomments, allowduplication, regex, test) =>
            {
                #region Additional custom validations/warnings.

                if (String.IsNullOrEmpty(xpath) | String.IsNullOrEmpty(jpath))
                {
                    #region Show a preventive warning if find & replace are exactly the same.
                    string tmpFind = (matchcase) ? find : find.ToLower();
                    string tmpReplace = (matchcase) ? replace : replace.ToLower();
                    if (tmpFind == tmpReplace & !allowduplication)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine("[WARNING] The --find and --replace value cancel each other out.");
                        Console.WriteLine("          Use the --matchcase and/or --allowduplication flag in order to match any results.");
                        Console.ResetColor();
                        DrawLine();
                    }
                    #endregion

                    #region Show a preventive warning if an xpath is provided together with section and/or key (the latter being useless in that case).
                    if (!String.IsNullOrEmpty(xpath))
                    {
                        if (!String.IsNullOrEmpty(key) | !String.IsNullOrEmpty(section))
                        {
                            key = null;
                            section = null;

                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("[WARNING] Since an --xpath is provided and thus an XML based file is assumed,");
                            Console.WriteLine("          --section and --key are ignored.");
                            Console.ResetColor();
                            DrawLine();
                        }
                    }
                    #endregion

                    #region Show a preventive warning if a jpath is provided together with section and/or key and/or ignore comments flag (the latter being useless in that case).
                    if (!String.IsNullOrEmpty(jpath))
                    {
                        if (!String.IsNullOrEmpty(key) | !String.IsNullOrEmpty(section) | ignorecomments)
                        {
                            key = null;
                            section = null;
                            ignorecomments = false;

                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine("[WARNING] Since an --jpath is provided and thus a JSON based file is assumed,");
                            Console.WriteLine("          --section, --key and --ignorecomments are ignored.");
                            Console.ResetColor();
                            DrawLine();
                        }
                    }
                    #endregion
                }
                else
                {
                    #region Show an error message if both an xpath and jpath are provided.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] Please only use either --xpath or --jpath.");
                    Console.ResetColor();
                    DrawLine();
                    Abort();
                    #endregion
                }

                #endregion

                #region Provided options summary.

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[Options]");

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("{0,-14} {1,-0}", "- Path", $"'{path.FullName.Replace(@"C:\Apps\Find And Replace Tool (KDb)\Build\Self-Contained", @"C:\Apps\FNR Tool")}'"); // [DevNote] Replacement for screenshot taking purposes.
                /*Console.WriteLine("{0,-14} {1,-0}", "", "-> Detected encoding: '" + DetectEncoding(path.FullName).EncodingName + "'");*/
                Console.WriteLine("{0,-14} {1,-0}", "- Find", $"'{find}'");
                Console.WriteLine("{0,-14} {1,-0}", "- Replace", $"'{replace}'");
                if (!String.IsNullOrEmpty(xpath))
                    Console.WriteLine("{0,-14} {1,-0}", "- XPath", $"'{xpath}'");
                if (!String.IsNullOrEmpty(jpath))
                    Console.WriteLine("{0,-14} {1,-0}", "- JPath", $"'{jpath}'");
                if (!String.IsNullOrEmpty(section) & String.IsNullOrEmpty(xpath))
                    Console.WriteLine("{0,-14} {1,-0}", "- Section", $"[{section}]");
                if (!String.IsNullOrEmpty(key) & String.IsNullOrEmpty(xpath))
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
                if (ignorecomments & String.IsNullOrEmpty(jpath))
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

                #region Prerequisites.
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

                #region Program logic.
                try
                {
                    // ------------------------------------
                    // Option A) Logic without XML or JSON:
                    // ------------------------------------
                    if (String.IsNullOrEmpty(xpath) & String.IsNullOrEmpty(jpath))
                        MatchAndReplace(path.FullName, find, replace, key, section, start, end, matchcase, matchword, ignorecomments, allowduplication, test);

                    // ------------------------------
                    // Option B) Logic with XML Path:
                    // ------------------------------
                    if (!String.IsNullOrEmpty(xpath) & String.IsNullOrEmpty(jpath))
                        MatchAndReplaceXml(path.FullName, find, replace, xpath, start, end, matchcase, matchword, ignorecomments, allowduplication, test);

                    // -------------------------------
                    // Option C) Logic with JSON Path:
                    // -------------------------------
                    if (!String.IsNullOrEmpty(jpath) & String.IsNullOrEmpty(xpath))
                    MatchAndReplaceJson(path.FullName, find, replace, jpath, start, end, matchcase, matchword, allowduplication, test);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[ERROR] " + e.Message);
                    Console.ResetColor();
                    DrawLine();

                    Abort();
                    throw;
                }

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

        private static void MatchAndReplace(string path, string find, string replace, string key, string section, string start, string end, bool matchcase, bool matchword, bool ignorecomments, bool allowduplication, bool test)
        {
            #region Read file.
            // Read the provided file.
            string sourceText = File.ReadAllText(path);

            // Temporary set replacedText = sourceText.
            // (Will be overwritten if a match based on provided options is found.)
            string replacedText = sourceText;
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
            // If the --allowduplication flag isn't provided and find value is found in replace value (case insensitive), wrap around:
            if (!allowduplication & replace.IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0)
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
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", end);   // If followed by provided string.
            else
                rgxPattern +=
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", "$");   // Else up to $ = end of text file.
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

            try
            {
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
                    Console.Write("{0,-0}", replace.ReplaceVarsInMatch(match));
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.ResetColor();

                    return match.Result(replace);
                }/*, options*/);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] " + e.Message);
                Console.ResetColor();
            }
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
            if (!test & matchCount != 0)
                File.WriteAllText(path, replacedText, DetectEncoding(path));

            DrawLine();
            #endregion
        }

        private static void MatchAndReplaceXml(string path, string find, string replace, string xpath, string start, string end, bool matchcase, bool matchword, bool ignorecomments, bool allowduplication, bool test)
        {
            #region Read file.
            // Load the provided xml based file with LineNumber & LinePosition support and set the root element.
            PositionXmlDocument xmlDoc = new PositionXmlDocument
            {
                PreserveWhitespace = true
            };

            try
            {
                /*XmlReaderSettings settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment
                };*/

                using (XmlReader reader = XmlReader.Create(path/*, settings*/))
                {
                    xmlDoc.Load(reader);
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] " + e.Message);
                Console.WriteLine("        Please fix the markup of the given file or resort to using other options.");
                Console.ResetColor();
                DrawLine();
                Abort();
            }

            XmlNode root = xmlDoc.DocumentElement;
            #endregion

            #region Regular Expression buildup.

            string rgxPattern;

            #region Explanatory breakdown (comment).
            /*
            ----------------------------------------------------------------------------------------------------------------------------
            Enhanced RegEx - Version v5d - All-encompassing (same base, although section & key trimmed & comment start changed to <!--).
            ----------------------------------------------------------------------------------------------------------------------------
            (?<=(?<between_start>START)[.\s\S]*)
            (?((?#exclude_replace_value)REPLACE)
                $donothing^
                |
                (?<!(?#exclude_trimmed_replace_value)NOTPRECEDEDBY)
                    (?<!(?#exclude_comment)<!--(.*?))
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

            #region Step 2) Ignore comments?
            if (ignorecomments)
                rgxPattern = @"(?<!(?#exclude_comment)<!--(.*?))"   // If line contains a '<!--' comment start.
                      + rgxPattern;
            #endregion

            #region Step 3) Avoid duplication?
            // If the --allowduplication flag isn't provided and find value is found in replace value (case insensitive), wrap around:
            if (!allowduplication & replace.IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                #region In-depth (comment).
                /* See MatchAndReplace function. */
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

            #region Step 4) Find between.
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
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", end);   // If followed by provided string.
            else
                rgxPattern +=
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", "$");   // Else up to $ = end of text file.
            #endregion

            #region Step 5) Add regex options.
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

            XmlNodeList nodeList = null;

            try
            {
                nodeList = root.SelectNodes(xpath);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] The provided XML path {xpath} is invalid.");
                Console.WriteLine("        " + e.Message);
                Console.ResetColor();
                DrawLine();

                Abort();
            }

            int matchCount = 0;

            foreach (XmlNode node in nodeList)
            {
                string outerXml = Regex.Replace(node.OuterXml, rgxPattern, (match) =>
                {
                    matchCount++;

                    var elem = (PositionXmlElement) node;

                    int line = elem.LineNumber + LineFromPos(node.OuterXml, match.Index) - 1; // OuterXml + Minus 1 in order to avoid the first line of root.OuterXml not being counted twice.

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
                    Console.Write("{0,-0}", replace.ReplaceVarsInMatch(match));
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.ResetColor();

                    return match.Result(replace);
                });

                // Get a fragment and slide the changed data into it.
                XmlDocumentFragment fragment = xmlDoc.CreateDocumentFragment();
                fragment.InnerXml = outerXml;

                // Replace the contents of the editNode with the user fragment.
                node.ParentNode.ReplaceChild(fragment, node);
            }

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
            if (!test & matchCount != 0)
                xmlDoc.Save(path);

            DrawLine();
            #endregion
        }

        private static void MatchAndReplaceJson(string path, string find, string replace, string jpath, string start, string end, bool matchcase, bool matchword, bool allowduplication, bool test)
        {
            #region Serialize/Deserialize settings.
            // Settings will automatically be used by JsonConvert.SerializeObject/DeserializeObject.
            // List of possible settings: https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateParseHandling = DateParseHandling.None,
                StringEscapeHandling = StringEscapeHandling.Default,
                Culture = CultureInfo.InvariantCulture,         // = JSON.Net Default.
                FloatParseHandling = FloatParseHandling.Double  // = JSON.Net Default.
            };
            #endregion

            #region Read file.
            // Read the provided json based file.
            string jsonSource = File.ReadAllText(path);

            JObject json = new JObject();
            try
            {
                json = JObject.Parse(jsonSource, new JsonLoadSettings
                {
                    LineInfoHandling = LineInfoHandling.Load
                });
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[ERROR] " + e.Message);
                Console.WriteLine("        Please fix the markup of the given file or resort to using other options.");
                Console.ResetColor();
                DrawLine();

                Abort();
            }
            #endregion

            #region Regular Expression buildup.

            string rgxPattern;

            #region Explanatory breakdown (comment).
            /*
            ----------------------------------------------------------------------------------------------------------------------------
            Enhanced RegEx - Version v5d - All-encompassing (same base, although section, key & comment trimmed).
            ----------------------------------------------------------------------------------------------------------------------------
            (?<=(?<between_start>START)[.\s\S]*)
            (?((?#exclude_replace_value)REPLACE)
                $donothing^
                |
                (?<!(?#exclude_trimmed_replace_value)NOTPRECEDEDBY)
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

            #region Step 2) Avoid duplication?
            // If the --allowduplication flag isn't provided and find value is found in replace value (case insensitive), wrap around:
            if (!allowduplication & replace.IndexOf(find, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                #region In-depth (comment).
                /* See MatchAndReplace function. */
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

            #region Step 3) Find between.
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
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", end);   // If followed by provided string.
            else
                rgxPattern +=
                      @"(?=[.\s\S]*?(?<between_end>END))".Replace("END", "$");   // Else up to $ = end of text file.
            #endregion

            #region Step 4) Add regex options.
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

            IEnumerable<JToken> jTokens = null;
            int matchCount = 0;

            try
            {
                /*
                .\fnr --path ".\example.json" --jpath "$..book[?(@.isbn)].price" --find "22" --replace "21.99" --test
                .\fnr --path ".\example.json" --jpath "$..book[?(@.isbn)].price" --find "22" --replace "21,99" --test
                .\fnr --path ".\example.json" --jpath "$..book[?(@.isbn)].releasedate" --find "1954" --replace "1854" --test
                */
                try
                {
                    jTokens = json.SelectTokens(jpath);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] The provided JSON path {jpath} is invalid.");
                    Console.WriteLine("        " + e.Message);
                    Console.ResetColor();
                    DrawLine();

                    Abort();
                }

                foreach(JToken jToken in jTokens)
                {
                    IEnumerable<JValue> jValues = jToken.GetLeafValues();
                    foreach(JValue jValue in jValues)
                    {
                        JValue jValueBackup = jValue;
                        Type expectedValueType = jValue.Value.GetType();
                        try
                        {
                            // Try match, set & convert to required type.
                            //jValue.Value = Convert.ChangeType({replacement}, expectedValueType, CultureInfo.InvariantCulture);
                            // [DevNote] TODO: For f.e. a double: works well with f.e. 21,99 but results in 2199 when replaced with 21.99 on my end. Depends on culture. Try autodetect?
                            //           NOTE: Adding InvariantCulture does the opposite (replaces fine with . but not with ,). => Note: Invariant culture is default when >reading< JSON.
                            //           POSSIBLE CONSENSUS: Replace with .
                            //                               If file really needs to contain f.e. prices with , => Run FNR again with f.e. --find "." --replace "," or a more specific regex.

                            ////jValue.Value = JsonConvert.DeserializeObject<JValue>(jValue.Value.ToString());
                            // [DevNote] Still adds \r\n\t\t f.e. (control characters) => possible to remove these? => cleaner with, but changes document a bit.

                            // A
                            string value = JsonConvert.SerializeObject(jValue.Value);
                            // B
                            //string value = jValue.Value.ToString(); // No need to serialize?
                            // C
                            //string value = JsonConvert.DeserializeObject(jValue.Value.ToString()).ToString();

                            value = Regex.Replace(value, rgxPattern, (match) =>
                            {
                                matchCount++;

                                if (double.TryParse(replace.ToString(), out double result))
                                {
                                    replace = replace.Replace(',', '.'); // Replace commas by dots ONLY when parsing to a double succeeded.
                                }

                                // Show match:
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.WriteLine("-> Match found: ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write("{0,-45} ", $"   {jValue.Path}:");
                                Console.Write("{0,-0}", match.Groups["context_left"].Value.TrimStart());
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write("{0,-0}", match.Value);
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                                Console.Write("{0,-0}", $" (Type: '{expectedValueType.Name}')");
                                Console.WriteLine();
                                // Show replacement:
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.WriteLine("=> Replaced: ");
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write("{0,-45} ", $"   {jValue.Path}:");
                                Console.Write("{0,-0}", match.Groups["context_left"].Value.TrimStart());
                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                Console.Write("{0,-0}", replace.ReplaceVarsInMatch(match));
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write("{0,-0}", match.Groups["context_right"].Value.TrimEnd());
                                //Console.Write("{0,-0}", $" (Type: '{expectedValueType.Name}')");
                                Console.WriteLine();
                                Console.WriteLine();
                                Console.ResetColor();

                                return match.Result(replace);
                            });

                            // Try converting the replace value to the expected value type.
                            if (expectedValueType.Name?.IndexOf("int", StringComparison.OrdinalIgnoreCase) >= 0)    // [DevNote] Case insensitive str.Contains() .
                            {
                                Console.WriteLine(value);
                                // F.e. if original value is int=10 and replace value is double=9.95 => could be an intentional change.
                                // F.e. if original value is int=10 and replace value is "not a number" => throw an error.
                                if (double.TryParse(value, out double result)) // If replace value is parsable as a double, set the new type to global setting default ('Double' or 'Decimal').
                                {
                                    // Replace commas by dots ONLY when parsing to a double succeeded.
                                    // A
                                    //jValue.Value = Convert.ChangeType(JsonConvert.DeserializeObject<JToken>(value), expectedValueType, CultureInfo.InvariantCulture);
                                    // B
                                    // No need to deserialize?
                                    jValue.Value = Convert.ChangeType(value, Type.GetType("System." + JsonConvert.DefaultSettings().FloatParseHandling.ToString()), CultureInfo.InvariantCulture); // [DevNote] Convertion of current global JSON Net setting to System.Type.
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                    jValue.Value = Convert.ChangeType(value, expectedValueType, CultureInfo.InvariantCulture);
                            }

                        }
                        catch
                        {
                            // Restore backup in case the replacement value doesn't match the required value type.
                            jValue.Value = jValueBackup.Value;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("[ERROR] The replacement doesn't match the required value type.");
                            Console.WriteLine($"        Expected value type = '{expectedValueType.Name}'.");
                            Console.ResetColor();

                            Abort();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] The provided JSON path {jpath} is invalid.");
                Console.WriteLine("        " + e.Message);
                Console.ResetColor();
                DrawLine();

                Abort();
            }

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
            if (!test & matchCount != 0)
                // A
                //File.WriteAllText(path, JsonConvert.SerializeObject(json), DetectEncoding(path));
                // B
                File.WriteAllText(path, json.ToString(), DetectEncoding(path)); // ? No need anymore to serialize because of using extension class and setting JToken values directly.
                // C Normally, it should be serialize, though preferring raw values (in order not to edit file):
                //File.WriteAllText(path, JsonConvert.DeserializeObject(json.ToString()).ToString(), DetectEncoding(path));
            
            DrawLine();
            #endregion
        }

        private static string ReplaceVarsInMatch(this string str, Match match)
        {
            return str.Replace("${between_start}", match.Groups["between_start"].Value)
                      .Replace("${section_lbchar}", match.Groups["section_lbchar"].Value)
                      .Replace("${section}", match.Groups["section"].Value)
                      .Replace("${section_rbchar}", match.Groups["section_rbchar"].Value)
                      .Replace("${key_indent}", match.Groups["key_indent"].Value)
                      .Replace("${key}", match.Groups["key"].Value)
                      .Replace("${in_between_key_value}", match.Groups["in_between_key_value"].Value)
                      .Replace("${context_left}", match.Groups["context_left"].Value)
                      .Replace("${value}", match.Groups["value"].Value)
                      .Replace("${context_right}", match.Groups["context_right"].Value)
                      .Replace("${ending}", match.Groups["ending"].Value)
                      .Replace("${between_end}", match.Groups["between_end"].Value);
        }

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

        /// <summary>
        /// Removes a string from end of an input string.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <param name="suffixToRemove">String to be removed from end of input string.</param>
        /// <returns>The input string with the suffix removed or the original input string.</returns>
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

        /// <summary>
        /// Application exit.
        /// </summary>
        private static void Abort()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Process aborted.");
            // Stop further execution (quit application).
            Environment.Exit(0);
        }

        #endregion
    }
}
