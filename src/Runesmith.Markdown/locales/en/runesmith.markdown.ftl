### The Markdown plugin's text in English, the source every translation starts from. The build turns each message into a member of the
### Strings class, such as Strings.OpenPreviewTitle.

## Commands and settings.

# The group of the plugin's commands in the command palette, and the category of its settings.
markdown-category = Markdown
open-preview-title = Open Preview
open-preview-description = Shows the Markdown file as it renders, beside the editor.
setting-sync-scroll = Scroll the preview with the caret
setting-sync-scroll-description = The preview scrolls to the heading, paragraph or other block the caret is in.
setting-refresh-delay = Preview refresh delay
setting-refresh-delay-description = How long typing pauses, in milliseconds, before the preview shows the change.
setting-continue-lists = Continue lists
setting-continue-lists-description = Enter in a list item starts the next item; Enter in an empty item ends the list.

## The preview panel.

# The title of the preview panel.
preview-title = Markdown Preview
# Shown in the panel while no Markdown file is open.
preview-empty = Open a Markdown file to preview it.
# Shown in the panel instead of the document when it cannot be rendered. $reason is the error's own text.
preview-failed = The preview could not be rendered: { $reason }

## Shown in the preview in place of an image that cannot be shown. $image is the image's address as the Markdown file writes it.

image-not-found = Image not found: { $image }
image-remote = Images from the web are not loaded: { $image }
image-unsupported = This kind of image cannot be shown: { $image }
# $megabytes (Number) - the largest size an image may have
image-too-large = Image larger than { $megabytes } MB: { $image }
image-unreadable = The image could not be read: { $image }

## Notifications after a click on a link in the preview. The link or the file's path is the notification's second line.

link-not-found = The linked file does not exist
link-unsupported = This kind of link cannot be opened
link-failed = The link could not be opened
