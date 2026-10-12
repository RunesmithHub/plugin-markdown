// The page of the Markdown preview. The plugin renders the Markdown and sends it here as HTML; this script shows it, colors its code
// blocks, scrolls to the line the plugin names, and tells the plugin which link was clicked. Preview/PreviewPage.cs is the other end.
(() => {
  'use strict';

  const content = document.getElementById('content');

  const showNotice = (text, isError) => {
    const notice = document.createElement('p');
    notice.className = isError ? 'notice error' : 'notice';
    notice.textContent = text;
    content.replaceChildren(notice);
  };

  // A block whose language highlight.js does not know stays as plain text.
  const highlightCode = () => {
    if (!globalThis.hljs) return;
    for (const block of content.querySelectorAll('pre code[class*="language-"]')) {
      const language = [...block.classList].find((name) => name.startsWith('language-')).slice('language-'.length);
      if (hljs.getLanguage(language)) hljs.highlightElement(block);
    }
  };

  const showDocument = (html) => {
    content.innerHTML = html;
    highlightCode();
  };

  // The last block that starts on or before the line is the one that holds it.
  const blockAt = (line) => {
    let found = null;
    for (const block of content.querySelectorAll('[data-line]')) {
      if (Number(block.dataset.line) > line) break;
      found = block;
    }
    return found;
  };

  const scrollToLine = (line) => {
    const block = blockAt(line);
    if (!block) return;
    const bounds = block.getBoundingClientRect();
    const inView = bounds.top >= 0 && bounds.top < window.innerHeight - 40;
    if (!inView) block.scrollIntoView({ block: 'center' });
  };

  const setTheme = (dark) => {
    document.documentElement.dataset.theme = dark ? 'dark' : 'light';
    document.getElementById('code-light').disabled = dark;
    document.getElementById('code-dark').disabled = !dark;
  };

  runesmith.onMessage((message) => {
    switch (message.kind) {
      case 'document': showDocument(message.html); break;
      case 'message': showNotice(message.text, false); break;
      case 'error': showNotice(message.text, true); break;
      case 'scroll': scrollToLine(message.line); break;
      case 'theme': setTheme(message.dark); break;
    }
  });

  // A link to a heading of the document scrolls to it; the plugin opens every other link, so the page itself never leaves.
  document.addEventListener('click', (event) => {
    const link = event.target.closest?.('a[href]');
    if (!link) return;
    event.preventDefault();
    const href = link.getAttribute('href');
    if (href.startsWith('#')) {
      let id = href.slice(1);
      try { id = decodeURIComponent(id); } catch { /* A fragment that is not percent-encoded is the id itself. */ }
      document.getElementById(id)?.scrollIntoView();
    } else {
      runesmith.postMessage({ kind: 'link', href });
    }
  });
})();
