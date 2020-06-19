<img align="right" width="60" height="58" src="https://i.imgur.com/j4FZIQO.gif">

# Find And Replace Tool (FNR)

## Download

**[ [ Latest Version ](https://github.com/KimDebroye/Find-And-Replace-Tool/releases) ]**

**<details><summary>` [ (Show|Hide) Developer Notes ] `</summary>**

## Developer Notes
- Although having a similar base concept, this project is in no way affiliated with a similarly named GitHub project. Although props to the author of that tool, I mainly wrote this tool since I was missing some features and user-friendly calls in the similarly named tool.
- The initial release/build of Find And Replace Tool:
  - is what I like to call a Prerefactor version/release, as in I initially coded and tested the ideas I came up with, without worrying about code refactoring.
    - In a possible later stage, there could be room for code refactoring and additional features described hereafter, together with even more additional features like f.e. directory & file masking support.
  - contains the current state at release of some experimental features I've been working on (*`--xpath` & `--jpath`*).
    - Although usable for testing purposes, these features are currently hidden and not included in the ReadMe nor in *`fnr --help`*, since I didn't feel they're ready for general usage yet (*XPath perhaps is, but since JSON is type sensitive, I'm strongly thinking of requiring a replacement type option as an addition.*).
- The builds should be publishable for **Windows, Linux & macOS**, since the code is written using .Net Core framework in Visual Studio 2019.
  - *Although being able to build for Linux & macOS, these builds are untested,<br />since I coded this on a Windows-only PC build*.
  - *Feel free to correct me if I'm wrong.*

</details>

## At a glance

![FNR Tool - At a glance](https://i.imgur.com/EKplhxL.gif)

**<details><summary>` [ (Show|Hide) FNR Tool Key Points ] `</summary>**

### ➥ In a nutshell

- **Command line based**.
  - *Uses [System.CommandLine](https://github.com/dotnet/command-line-api) as a code base.*
- **Allows for quick, safe, clean & easy to understand text replacements**.
  - **In entire file or at specific locations in file** only.<br />
    (*f.e. in given key, in given section, ...*)
  - **In plain and/or config based text** documents.<br />
    (*`.cfg`, `.ini`, `.conf`, `.txt`, ...*)
  - **No need to (*f.e.*) move/copy around text/config files** by editing them directly instead.
    - *Avoids (f.e.) overwriting config files and possibly removing any previous edits.*
  - **No need to worry about the config format**.
    - By default, disregards whether values are surrounded by other characters,<br />
      single and/or double quotes, any kind of white space and/or any other special characters<br />
      (*f.e. presence or absence of f.e. equal sign when limiting search to be paired with keys*).
  - **Automatically respects file encoding**.
    - *Uses a [port of Mozilla Universal Charset Detector](https://github.com/errepi/ude) as a helper library.*
- **Easily callable with easy to understand options and flags**.<br />
  (*from command line or f.e. a `.bat`, `.cmd` or `.ps1` file*)
  - Complete separation of behind-the-scenes logic & calls.
  - A fine degree of customizability.
- **Uses a behind-the-scenes generated, all-encompassing regular expression pattern**,<br />
  *custom written and tested for various pitfalls by author ([KDb](https://discord.com/users/536322521079742464)) & beta-tester ([Enkak](https://discord.com/users/516621454092271617))*.

### ➥ This may be a good tool for people that

- seek for a quick way to configure any kind of backend,
- aren't in the mood or may not have the time or skills (yet)<br />
  to program a custom application based setup,
- have some basic knowledge of calling/using/working with<br />
  a command line interface.

</details>

 

## Quick Usage Examples

**<details><summary>` [ (Show|Hide) Full copy of` *example.txt* `] `</summary>**

``` ini
# ---------------------------------------------------------------
# Example of a plain English text, copied from www.loremipsum.io.
# ---------------------------------------------------------------

Lorem ipsum, or lipsum as it is sometimes known, is dummy text used in laying out print, graphic or web designs. The passage is attributed to an unknown typesetter in the 15th century who is thought to have scrambled parts of Cicero's De Finibus Bonorum et Malorum for use in a type specimen book. It usually begins with:

"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua."

The purpose of lorem ipsum is to create a natural looking block of text (sentence, paragraph, page, etc.) that doesn't distract from the layout. A practice not without controversy, laying out pages with meaningless filler text can be very useful when the focus is meant to be on design, not content.

The passage experienced a surge in popularity during the 1960s when Letraset used it on their dry-transfer sheets, and again during the 90s as desktop publishers bundled the text with their software. Today it's seen all around the web; on templates, websites, and stock designs.
```
</details>

**<details><summary>` [ (Show|Hide) Full copy of` *example.ini* `] `</summary>**
```
# ==========================================================================
# Note: The below example contains a mix of config formats.
#       Normally however, if a config file is well written,
#       its format would be uniform.
# ==========================================================================

# -----------------------------------------------------------------
# Example of an intentionally not so well formatted custom section.
# -----------------------------------------------------------------
[Custom Section]
key=value
truely_great_setting = "Quite a setting."
dummy_setting_v="enabled"
dummy_setting_w = "Enabled"
dummy_setting_x = "enabled"
dummy_setting_y =   disabled
dummy_setting_z   enabled
#menu_layout	= "Cool Theme"
menu_layout		: "Cool Theme Cool Theme Theme Cool"
menu_layout		= "CoolSuper Theme"
	menu_layout		= "SuperCool Theme"
menu_layout		= "Cool Theme - Dark Mode"
menu_layout		= "SuperCool Theme - Dark Mode Cool Theme"
no_menu_layout	= "SuperCool Theme"
menu_layout		   Cool Theme
settings_show_audio = "true"


# ==========================================================================
# Note: The below examples are excerpts taken from frontend/backend configs.
#       [Sections] are added as an example only and aren't present
#       in their respective configs (=> considered as 'global' key/values).
# ==========================================================================

# ----------------------
# RetroArch.cfg excerpt.
# ----------------------
[RetroArch]
settings_show_audio = "true"
settings_show_configuration = "true"
settings_show_core = "true"
settings_show_directory = "true"
settings_show_drivers = "true"


# -------------------------------
# RetroArch Core Options excerpt.
# -------------------------------
[RetroArch Core Options]
gambatte_gb_colorization = "auto"


# --------------------
# Attract.cfg excerpt.
# --------------------
display	System X
	layout               Cool Theme
	romlist              System X
	in_cycle             yes
	in_menu              yes
    # (...)

display	System Y
	layout               Cool Theme
	romlist              System Y
	in_cycle             yes
	in_menu              yes
    # (...)
		
general
    # (...)
	menu_layout 		 Cool Theme


# --------------------------------
# RetroFE controller.conf excerpt.
# --------------------------------
[RetroFE]
up = Up,joy0Hat0Up,joy1Hat0Up
down = Down,joy0Hat0Down,joy1Hat0Down
left = Left,joy0Hat0Left,joy1Hat0Left
right = Right,joy0Hat0Right,joy1Hat0Right
pageUp = A
pageDown = B
letterUp = joy0Button4,joy1Button4
letterDown = joy0Button5,joy1Button5
nextPlaylist = P
addPlaylist = I
removePlaylist = O
random = R
select = Return,joy0Button2,joy1Button2
back = Escape,joy0Button3,joy1Button3
quit = Q,joyButton10


# -----------------
# Mame.ini excerpt.
# -----------------
[MAME]
autoframeskip             0
frameskip                 0
seconds_to_run            0
throttle                  1
syncrefresh               0
sleep                     1
speed                     1.0
refreshspeed              0
lowlatency                0
```
</details>

---

**<details><summary>`[ (Show|Hide) Example 1 ]`</summary>**

> **Summary**
- In given *`ini file`*, find and replace all occurrences of "*`Cool`*" with "*`SuperCool`*", within section "*`Custom Section`*" and paired with key "*`menu_layout`*" while ignoring comments.

> **Input:**
``` bat
.\fnr --path ".\example.ini" --find "Cool" --replace "SuperCool" --section "Custom Section" --key "menu_layout" --ignorecomments --test
```

> **Output:**

![FNR Tool Example: Cool -> SuperCool](https://i.imgur.com/ef17cju.gif)

> **❓ Why aren't all occurrences of "Cool" matched in the above example?**
- Reasons for not being matched:
  - The `--ignorecomments` flag is used.
    - Hence ignores f.e. *`#menu_layout	= "Cool Theme"`*.
  - A `--key "menu_layout"` requirement is given.
    - Hence ignores f.e. *`no_menu_layout = "SuperCool Theme"`*.
  - A `--section "Custom Section"` requirement is given.
    - Hence ignores f.e. *`menu_layout	Cool Theme`*.
  - *`example.ini`* already contains the "*SuperCool*" replace value.
    - Hence ignores f.e. *`menu_layout = "SuperCool Theme"`*.<br />
      (=> Would otherwise become "*SuperSuperCool*", which may not be desired.)
    - By default, FNR Tool avoids duplication behavior.
    - The `--allowduplication` flag can be used in order to toggle this behavior.

> **Occurrences not matched in *`example.ini`*:**
``` ini
(...)
[Custom Section]
(...)
#menu_layout	= "Cool Theme"
	menu_layout		= "SuperCool Theme"
menu_layout		= "SuperCool Theme - Dark Mode (...)"
no_menu_layout	= "SuperCool Theme"
(...)
# --------------------
# Attract.cfg excerpt.
# --------------------
display	System X
	layout   Cool Theme
# (...)

display	System Y
	layout   Cool Theme
# (...)
		
general
# (...)
	menu_layout 		 Cool Theme
(...)
```

</details>

---

**<details><summary>`[ (Show|Hide) Example 2 ]`</summary>**

> **Summary**
- In given *`ini file`*, find and replace all occurrences of "*`Cool Theme`*" with "*`Another Cool Theme`*", paired with key "*`layout`*" while restricting search between (*interpreted as regex*) "*`display(zero or more white space characters)System Y`*" and "*`general`*" (*since there's no ini section defined for that region*).

> **Input:**
``` bat
.\fnr --path ".\example.ini" --find "Cool Theme" --replace "Another Cool Theme" --key "layout" --start "display\s*System Y" --end "general" --regex --test
```

> **Output:**

![FNR Tool Example: Attract config with regex](https://i.imgur.com/PxJzWcS.gif)

> **❓ Is using a regular expression necessary?**
- Short answer: no.
  - Even though in this case it would/could be more precise, it's merely to demonstrate:
    - the usage of the `--regex` flag and
    - the pitfall of not noticing there is or might be<br />more than once space and/or tab character in between "*display*" and "*System Y*",<br />
      which would result in zero matches.

> **Input alternatives:**

**▸ Without regular expression**:
- (*Without `--regex` flag*.)
- (*Start looking from `--start "System Y"` instead*.)
``` bat
.\fnr --path ".\example.ini" --find "Cool Theme" --replace "Another Cool Theme" --key "layout" --start "System Y" --end "general" --test
```

**▸ Without regular expression & from "System Y" up to (end of file)**:
- (*Without `--regex` flag*.)
- (*Without `--end` option*.)
  - **Be careful with replacements over a larger region of text when not familiar with a given config file.<br />It could cause unwanted replacements.**
    - *F.e. at the very end of the file there could be another occurrence of `layout     Cool Theme`<br />that would be unwillingly replaced (and could go unnoticed at first)*.
``` bat
.\fnr --path ".\example.ini" --find "Cool Theme" --replace "Another Cool Theme" --key "layout" --start "System Y" --test
```

</details>

 

## Usage

![FNR Tool - Usage](https://i.imgur.com/Vm9wbwN.gif)

### ➥ General usage
---
- **Run from within current (working) directory**:
  - **`(.\)fnr(.exe) [options|flags]`**
    - *Run `fnr(.exe)` from within current directory `.\`*
    - *with given `options | flags` (order doesn't matter).*
- **Run from within absolute directory**:
  - **`C:\FNR Tool\fnr(.exe) [options|flags]`**
    - *Run `fnr(.exe)` from within absolute directory `C:\FNR Tool\`*
    - *with given `options | flags` (order doesn't matter).*
- **Run from within relative directory**:
  - **`..\..\FNR Tool\fnr(.exe) [options|flags]`**
    - *Run `fnr(.exe)` from within relative directory `..\..\FNR Tool\`*<br />
      *(..\\..\ = parentdir\parentdir\ = "up" 2 directories, then from within FNR Tool subdirectory)*
    - *with given `options | flags` (order doesn't matter).*

> **[ Important ]**<br />
> **Relative paths should be:**<br />
> • **relative to the file calling FNR Tool**;<br />
> • **relative to the current working directory when calling FNR Tool from Command Prompt**.

 

### ➥ Ways of calling **`Find And Replace Tool`**
---

(*Basically: every possible solution able to call an executable with arguments.*)

#### • Quick Start/Test/Preview (*Windows specific*):

**<details><summary>` [ (Show|Hide) Command Prompt call from` *_FNR Command Line Interface* `shortcut ] `</summary>**

- Each release package comes bundled with<br />
  - an *`example.ini`* and *`example.txt`* file &<br />
  - a *`_FNR Command Line Interface`* shortcut that:
    - launches a command window from current directory,
    - calls the **Find And Replace Tool** (*`fnr.exe`*) with its in-built `--help` option &
    - waits for any additional user input.
- This can be useful should anyone:
  - want to hands-on follow along or fiddle with the examples shown on this page or<br />
  - do so some testing for a personal project first.
- If file overwrites aren't desired (*preview only*),<br />
  each call could be provided with the `--test` option.

**Folder structure**
```
.\fnr.exe
.\example.ini
.\_FNR Command Line Interface shortcut
```
</details>

#### • From file (*Windows specific*):

**<details><summary>` [ (Show|Hide) Command Prompt call from` *.bat* `or` *.cmd* `file ] `</summary>**

**Folder structure example**
```
.\fnr.exe
.\example.ini
.\Call Examples\fnr.bat
```
**`fnr.bat` content example**
``` bat
:: Turns off command echoing:
@echo off

:: FNR Call:
:: (Use start /min "" ..\fnr.exe to hide FNR Tool output.)
..\fnr.exe --path "..\example.ini" --find "Cool Theme" --replace "Cool Theme - Lite Mode" --key "menu_layout" --section "Custom Section" --test

:: Comment or remove to auto close:
pause>nul
```

![FNR Tool - Call from .bat or .cmd file](https://i.imgur.com/6Y33q7a.gif)
</details>

**<details><summary>` [ (Show|Hide) PowerShell call from` *.ps1* `file ] `</summary>**

**Folder structure example**
```
.\fnr.exe
.\example.ini
.\Call Examples\fnr.ps1
```
**`fnr.ps1` content example**
``` powershell
# Bypass execution policy:
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope CurrentUser

# FNR Call:
..\fnr.exe --path "..\example.ini" --find "Cool Theme" --replace "Cool Theme - Lite Mode" --key "menu_layout" --section "Custom Section" --test

# Comment or remove to auto close:
cmd /c pause | out-null
```

![FNR Tool - Call from .ps1 file](https://i.imgur.com/LZLGEja.gif)
</details>

#### • ...

 

### ➥ Options
---

#### 🔘 `--path` | `-p` "absolute-or-relative-path-to-file.extension"
- **Required**.
- To be provided with an absolute or relative valid text file path.
  - *Aborts with an error when a given file isn't found or supported.*

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Relative paths should be**:
    - relative to the file calling FNR Tool;
    - relative to the current working directory when calling FNR Tool from Command Prompt.
</details>

##### Syntax
``` bat
.\fnr --path ".\example.ini" [...]
```

---

#### 🔘 `--find` | `-f` "text value to be found"
- **Required**.
- To be provided with a text value to be matched.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Case insensitive**.
    - F.e. `--find "IpSuM"`
      - *Would match "**IPSUM**", "**ipsum**", ...*.
    - *Use `--matchcase` flag in order to toggle this behavior*.
  - **Matches any occurrence of given text** (*except see bullet point hereafter*).
    - F.e. `--find "ipsum"`
      - *Would match "**ipsum**", "l**ipsum**", "lorem**ipsum**dolor", ...*.
    - *Use `--matchword` flag in order to toggle this behavior*.
  - **Won't match an occurrence of given text if it is equal to or part of the replacement text**.
    - F.e. `--find "Cool Theme" --replace "Cool Theme - Dark Mode"`
      - *Would match "**Cool Theme**", "Super **Cool Theme**", "**Cool Theme** by", ...*.
      - *Would **NOT** match "Cool Theme - Dark Mode" already present in file*,<br />
        *in order to avoid text duplication when called again*.<br />
        (*Avoids f.e. having "Cool Theme - Dark Mode - Dark Mode - Dark Mode" in text file after more than 1 call*.)
    - *Use `--allowduplication` flag in order to toggle this behavior*.
  - **Matches any commented occurrence of given text**.
    - F.e. `--find "comment"`
      - *Would match "# This is a **comment** in text file.", "; This is an alternative **comment** in text file.", ...*.
    - *Use `--ignorecomments` flag in order to toggle this behavior*.
  - **Characters are escaped behind-the-scenes**.
    - F.e. `--find "auto|internal|custom"`
      - *Would match "**auto|internal|custom**"*.
      - *Would **NOT** match "auto", "internal" or "custom"*.
    - *Use `--regex` flag in order to toggle this behavior*.
</details>

##### Syntax
``` bat
.\fnr --find "lorem ipsum" [...]
```

---

#### 🔘 `--replace` | `-r` "text replacement value"
- **Required**.
- To be provided with a replacement text value for the matched results.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Always case sensitive**.
  - **Ability to use capture groups as replace value** (*combinable with other text*).
    - Useful when combined with `--regex` flag.
    - **Most important capture group replacements** (*see source code for other possibilities*):
      - `${value}` = **The matched result**.
        - *F.e.* `.\fnr --find "auto|internal|custom" --replace "hello ${value}" --regex [...]`
	    - **=>** Results in f.e. "*hello auto*", "*hello internal*", "*hello custom*", ...
      - `${section}` = **The section name**.
        - *F.e.* `.\fnr --find "auto|internal|custom" --replace "${value} in section [${section}]" --section "A|B" --regex [...]`
	    - **=>** Results in f.e. "*auto in section [A]*", "*internal in section [B]*", "*custom in section [B]*", ...
      - `${key}` = **The key name**.
        - *F.e.* `.\fnr --find "auto|internal|custom" --replace "${key} = ${value}" --key "setting_x|setting_y" --regex [...]`
	    - **=>** Results in f.e. "*setting_x = auto*", "*setting_y = internal*", "*setting_y = custom*", ...
      - `${context_left}` = **All characters preceding the matched value on the matched line**.
        - *F.e.* `.\fnr --find "enabled|lorem" --replace "${context_left}${value}" --regex [...]`
	    - **=>** Results in f.e. "   *setting_x   =   'enabled*", "*setting_y = enabled*", "*This is some cool text preceding lorem*", ...
      - `${context_right}` = **All characters succeeding the matched value on the matched line**.
        - F.e. `.\fnr --find "enabled|lorem" --replace "${value}${context_right}" --regex [...]`
	    - => Results in f.e. "*enabled'*", "*enabled*", "*lorem ipsum dolor sit amet.*", ...
</details>

##### Syntax
``` bat
.\fnr --replace "other text" [...]
```

---

#### 🔘 `--key` | `-k` "key"
- Optional & only useful for config based text files containing keys.
- To be provided with a key name in order to only look for matches in value(s) paired with given key.
- Can be used as stand-alone additional option or in combination with other additional options.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Disregards whether key values are surrounded by any kind of white space,
single and/or double quotes and/or any separation character (*f.e. equal sign*)**.
    - F.e. `--key "example_key"`
      - *Would match values in lines*:
        - *example_key=value*
        - *example_key value*
        - *example_key = value*
        - *example_key='value'*
        - *example_key 'value'*
        - *example_key = 'value'*
        - *example_key="value"*
        - *example_key "value"*
        - *example_key = "value"*
        - *example_key           value*
        - *example_key     =     value*
        -      *example_key           value*
        -      *example_key     =     value*
        - ...
  - **Case insensitive**.
    - F.e. `--key "eXaMpLe_KeY"`
      - *Would match values in keys "**example_key** = value", "**EXAMPLE_KEY** = value", ...*.
    - *Use `--matchcase` flag in order to toggle this behavior*.
  - **Matches any commented and uncommented occurrence of given key**.
    - F.e. `--key "example_key"`
      - *Would match values in keys "**example_key** = value", "#**example_key** = value", ";**example_key** = value", ...*.
    - *Use `--ignorecomments` flag in order to toggle this behavior*.
  - **Characters are escaped behind-the-scenes**.
    - F.e. `--key "example_key_1|example_key_2"`
      - *Would match values in keys "**example_key_1|example_key_2** = value"*.
      - *Would **NOT** match values in keys "example_key_1 = value" or "example_key_2 = value"*.
    - *Use `--regex` flag in order to toggle this behavior*.
  - **Word bound**.
    - Due to safety reasons (f.e. prevents accidental similar key value replacements).
    - F.e. `--key "example_key"`
      - *Would match values in keys "**example_key** = value", ...*.
      - *Would **NOT** match values in keys "example_key_1 = value", "other_example_key = value"*.
    - *Can be circumvented however by using the `--regex` flag and doing:*.
      - F.e. `--key ".*example_key.*" --regex`
      - *Would eventually match values in keys "**other_example_key** = value", "**example_key_1** = value", "**example_key_2** = value", ...*.
</details>

##### Syntax
``` bat
.\fnr --key "key_name" [...]
```

---

#### 🔘 `--section` | `-s` "section"
- Optional & only useful for config based text files containing sections.
- To be provided with a section name **without [brackets]** in order to only look for matches contained within a given section.
- Can be used as stand-alone additional option or in combination with other additional options.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Follows [bracketed section] standard**.
    - *Brackets are automatically wrapped around the section name behind-the-scenes.*
    - *Should a config file not follow the bracketed [Section] standard,*<br />
      *use `--start` and/or `--end` option(s) instead in order to limit search.*
  - **Case insensitive**.
    - F.e. `--section "example section"`
      - *Would match values in sections "**[example section]** ...", "**[EXAMPLE SECTION]** ...", ...*.
    - *Use `--matchcase` flag in order to toggle this behavior*.
  - **Matches any commented and uncommented occurrence of given section**.
    - F.e. `--section "example section"`
      - *Would match values in sections "**[example section]** ...", "#**[example section]** ...", ";**[example section]** ...", ...*.
    - *Use `--ignorecomments` flag in order to toggle this behavior*.
  - **Characters are escaped behind-the-scenes**.
    - F.e. `--section "example section|other section"`
      - *Would match values in sections "**[example section|other section]** ..."*.
      - *Would **NOT** match values in sections "[example section] ..." or "[other section] ..."*.
    - *Use `--regex` flag in order to toggle this behavior*.
  - **Word bound**.
    - Due to safety reasons (f.e. prevents accidental similar section value replacements).
    - F.e. `--section "example section"`
      - *Would match values in sections "**[example section]** ...", ...*.
      - *Would **NOT** match values in sections "[other example section], "[example section 1] ...", ...*.
    - *Can be circumvented however by using the `--regex` flag and doing:*.
      - F.e. `--section ".*example section.*" --regex`
      - *Would eventually match values in sections "**[other example section]** ...", "**[example section 1]** ...", ...*.
</details>

##### Syntax
``` bat
.\fnr --section "section name without brackets" [...]
```

---

#### 🔘 `--start` "look from here in text file"
- Optional.
- To be provided with a text value in order to set from where in file to start looking for matches.
  - *If not provided: automatically defaults to (from start of file)*.
- Can be used as stand-alone additional option or in combination with other additional options.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Case insensitive**.
    - F.e. `--start "from here"`
      - *Would start looking at "**from here** ...", "**FROM HERE** ...", ...*.
    - *Use `--matchcase` flag in order to toggle this behavior*.
  - **Characters are escaped behind-the-scenes**.
    - F.e. `--start "from here|or here"`
      - *Would start looking at "**from here|or here** ..."*.
      - *Would **NOT** start looking at "**from here** ..." or "**or here** ..."*.
    - *Use `--regex` flag in order to toggle this behavior*.
</details>

##### Syntax
``` bat
.\fnr --start "look from here in text file" [...]
```

---

#### 🔘 `--end` "look up to here in text file"
- Optional.
- To be provided with a text value in order to set up to where in file to end looking for matches.
  - *If not provided: automatically defaults to (up to end of file)*.
- Can be used as stand-alone additional option or in combination with other additional options.

**<details><summary>` [ (Show|Hide) In-depth information ] `</summary>**

- **By default**:
  - **Case insensitive**.
    - F.e. `--end "up to here"`
      - *Would end looking at "**up to here** ...", "**UP TO HERE** ...", ...*.
    - *Use `--matchcase` flag in order to toggle this behavior*.
  - **Characters are escaped behind-the-scenes**.
    - F.e. `--end "up to here|or here"`
      - *Would end looking at "**up to here|or here** ..."*.
      - *Would **NOT** end looking at "**up to here** ..." or "**or here** ..."*.
    - *Use `--regex` flag in order to toggle this behavior*.
</details>

##### Syntax
``` bat
.\fnr --end "look up to here in text file" [...]
```

 

### ➥ Flags
---

#### 🏳 `--matchcase` | `-c`
- Optional.
- Enables case sensitive matching of given option values.
- Comparative example:
  - **without `--matchcase` flag** (*default*):<br />`fnr --find "eXaMpLe" [...]`
    - *Would match "**example**", "**eXaMpLe**", "**EXAMPLE**", ...*.
  - **with `--matchcase` flag**:<br />`fnr --find "eXaMpLe" --matchcase [...]`
    - *Would match "**eXaMpLe**"*.
    - *Wouldn't match "example", "EXAMPLE", ...*.

---

#### 🏳 `--matchword` | `-w`
- Optional.
- Enables precise word matching of given `--find` value (*in other words: sets a word boundary*).
- Comparative example:
  - **without `--matchword` flag** (*default*):<br />`fnr --find "ipsum" [...]`
    - *Would match "**ipsum**", "l**ipsum**", "lorem**ipsum**dolor", ...*.
  - **with `--matchword` flag**:<br />`fnr --find "ipsum" --matchword [...]`
    - *Would match "**ipsum**"*.
    - *Wouldn't match "lipsum", "loremipsumdolor", ...*.

---

#### 🏳 `--ignorecomments` | `-i`
- Optional.
- Ignores matching values in commented lines.
  - A line is considered a comment when it starts with either **`#`** or **`;`**.
- Comparative example:
  - **without `--ignorecomments` flag** (*default*):<br />`fnr --find "comment" [...]`
    - *Would match "This isn't a **comment**.", "# This is a **comment**.", "; This is another **comment**.", ...*.
  - **with `--ignorecomments` flag**:<br />`fnr --find "comment" --ignorecomments [...]`
    - *Would match "This isn't a **comment**.", "Although this line contains a # character, this isn't a **comment** line."*.
    - *Wouldn't match "# This is a comment.", "; This is another comment.", ...*.

---

#### 🏳 `--allowduplication` | `-d`
- Optional.
- Find and replace even when text to be found is equal to or part of the replacement text.
- Comparative example:
  - **without `--allowduplication` flag** (*default*):<br />`fnr --find "Cool Theme" --replace "Cool Theme - Dark Mode" [...]`
    - *Would match "**Cool Theme**"*.<br />
      => *(after replacement:)*<br />
      *"**Cool Theme - Dark Mode**"*.
    - *Wouldn't match "Cool Theme - Dark Mode"*.<br />
      => *(will NOT replace, avoids:)*<br />
      ~~*"Cool Theme - Dark Mode - Dark Mode"*~~.
  - **with `--allowduplication` flag**:<br />`fnr --find "Cool Theme" --replace "Cool Theme - Dark Mode" --allowduplication [...]`
    - *Would match "**Cool Theme**"*.<br />
      => *(after replacement:)*<br />
      *"**Cool Theme - Dark Mode**"*.
    - *Would also match "**Cool Theme** - Dark Mode"*.<br />
      => *(after replacement:)*<br />
      *"**Cool Theme - Dark Mode** - Dark Mode"*.

---

#### 🏳 `--regex`
- Optional.
- Enables use of regular expressions in `--find`, `--key`, `--section`, `--start` & `--end` options.
- For more information:
  - [.Net Regular Expression Language - Quick Reference](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference ".Net Regular Expression Language - Quick Reference") 
- Comparative example:
  - **without `--regex` flag** (*default*):<br />`fnr --find "auto|internal|custom" [...]`
    - *Would match "**auto|internal|custom**"* to the letter.
    - *Would **NOT** match "auto", "internal" or "custom"*.
  - **with `--regex` flag**:<br />`fnr --find "auto|internal|custom" --regex [...]`
    - *Would **NOT** match "auto|internal|custom"* to the letter.
    - *Would match "**auto**", "**internal**" or "**custom**"*.

---

## Contact
- [Feel free to contact me at Discord.](https://discord.com/users/536322521079742464)

