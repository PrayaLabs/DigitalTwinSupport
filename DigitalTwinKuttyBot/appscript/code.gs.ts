// ==== CONFIG ====
// Run this once in Apps Script editor to save your API key securely:
// PropertiesService.getScriptProperties().setProperty('GEMINI_API_KEY', 'YOUR_GEMINI_KEY_HERE');

const MODEL = "gemini-1.5-flash"; // You can change to gemini-1.5-pro, etc.

function doGet(e) {
  try {
    const prompt = (e.parameter.q || "").trim();
    if (!prompt) return ContentService.createTextOutput("No query provided");

    const key = PropertiesService.getScriptProperties().getProperty('GEMINI_API_KEY');
    if (!key) return ContentService.createTextOutput("Missing API key");

    const url = `https://generativelanguage.googleapis.com/v1beta/models/${MODEL}:generateContent?key=${key}`;

    const payload = {
      contents: [{ parts: [{ text: prompt }] }]
    };

    const response = UrlFetchApp.fetch(url, {
      method: "post",
      contentType: "application/json",
      payload: JSON.stringify(payload),
      muteHttpExceptions: true
    });

    if (response.getResponseCode() !== 200) {
      return ContentService.createTextOutput("Error: " + response.getContentText());
    }

    const data = JSON.parse(response.getContentText());
    const text = data?.candidates?.[0]?.content?.parts?.[0]?.text || "No response";

    return ContentService.createTextOutput(text)
      .setMimeType(ContentService.MimeType.TEXT);

  } catch (err) {
    return ContentService.createTextOutput("Exception: " + err.message);
  }
}
