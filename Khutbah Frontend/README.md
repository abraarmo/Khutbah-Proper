# Khutbah Live Translator

A desktop tool that extracts Arabic text from PDF sermons (khutbahs) and prepares
it for translation into English.

## Current status

Early development. File input and Arabic text extraction are in place; translation
is the next phase.

## How it works

1. The user selects a PDF via a WinForms file browser (`OpenFileDialog`).
2. The PDF is sent to **Azure AI Document Intelligence** (`prebuilt-read` model),
   which performs OCR and returns the Arabic text in correct right-to-left reading order.
3. The extracted text is saved as a `.txt` file alongside the original PDF (UTF-8).

Azure OCR is used rather than plain PDF text extraction because Arabic PDFs frequently
store text as baked-in presentation-form glyphs in visual order, which standard
extractors return scrambled and reversed. OCR reads the rendered page and reconstructs
correct, joined, logical-order Arabic — and also handles scanned PDFs with no text layer.

## Tech stack

- C# / .NET (WinForms)
- Azure AI Document Intelligence (`Azure.AI.DocumentIntelligence`)

## Configuration

Azure credentials are read from `appsettings.json`, which is **git-ignored** and must
be created locally:

```json
{
  "AzureDocumentIntelligence": {
    "Endpoint": "https://<your-resource>.cognitiveservices.azure.com/",
    "ApiKey": "<your-key>"
  }
}
```

Never commit this file. Use the Azure portal to regenerate keys if one is ever exposed.

## Roadmap

- [x] PDF file input (WinForms)
- [x] Arabic text extraction via Azure OCR
- [ ] Arabic → English translation
- [ ] UI for viewing original + translation
- [ ] Swap WinForms for a proper front end (Vue.js)
