# PO to Google Translater Using GTKSharp

Dotnet tool which translate PO files using Google Translate API.

## Usage

USAGE: `robotranslator [--help] [--verbose] [--output <output>] [--source <output>] [--language <language>]
                      [--apikey <key>] [--first <count>]`

OPTIONS:

    --verbose, -v         Verbose output
    --output, -o <output> .po file where to save translation
    --source, -s <output> .po/.pot file to translate
    --language, -l <language>
                          Language to which translate file
    --apikey, -k <key>    API key for Google Translate
    --first <count>       Limit translation to only first non-translated entries
    --help                display this list of options.

Basically translation process for now is one of the following:

Same file translation of untranslated strings

```sh
robotranslator -s Game.po -l uk -k <api-key>
```

or translation of whole file

```sh
robotranslator -s Game.pot -o Game.po -l uk -k <api-key>
```

As of March 2025 Google Translate API provide 500 K free characters translation per months.
You can get your API key [here](https://cloud.google.com/translate)

## FromJson usage

Use syntax below to translate JSON files to PO/POT files.

USAGE: `robotranslator fromjson [--help] [--verbose] [--output <output>] [--source <output>] [--language <language>]`

OPTIONS:

    --verbose, -v         Verbose output
    --output, -o <output> .po/.pot file where to save translation
    --source, -s <output> .json file to translate
    --language, -l <language>
                          Language of the JSON file
    --help                display this list of options.

## References

* [Origional F# repo](https://github.com/kant2002/RoboTranslator)
