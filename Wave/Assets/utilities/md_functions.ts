export function updateCharactersLeft(input : HTMLInputElement) {
    const maxLength = input.maxLength;
    const currentLength = input.value.length;

    const newLeft = maxLength - currentLength;

    const parent = input.parentNode as HTMLElement;
    if (!parent) return;

    let elem = parent.querySelector(".characters-left") as HTMLElement;
    if (elem) {
        elem.innerText = `${newLeft}`;
    } else {
        parent.classList.add("relative");
        elem = document.createElement("span");
        elem.classList.add("characters-left");
        elem.innerText = `${newLeft}`;
        parent.appendChild(elem);
    }
}

export function insertBeforeSelection(target: HTMLTextAreaElement, markdown : string, startOfLine = false) {
    const start = target.selectionStart;
    const end = target.selectionEnd;
    const value = target.value;
    let doStart = start;
    if (startOfLine) {
        doStart = value.lastIndexOf("\n", start) + 1;
    }

    target.focus();
    target.value = value.substring(0, doStart) + markdown + value.substring(doStart);

    target.selectionStart = start + markdown.length;
    target.selectionEnd = end + markdown.length;
    target.focus();
    target.dispatchEvent(new Event("input", { bubbles: true }));
}

export function insertBeforeAndAfterSelection(target: HTMLTextAreaElement, markdown : string) {
    while (/\s/.test(target.value[target.selectionStart]) && target.selectionStart < target.value.length) {
        target.selectionStart++;
    }
    while (/\s/.test(target.value[target.selectionEnd - 1]) && target.selectionEnd > 0) {
        target.selectionEnd--;
    }

    const start = target.selectionStart;
    const end = target.selectionEnd;
    const value = target.value;

    target.focus();
    target.value = value.substring(0, start) +
        markdown + value.substring(start, end) + markdown +
        value.substring(end);

    target.selectionStart = start + markdown.length;
    target.selectionEnd = end + markdown.length;
    target.focus();
    target.dispatchEvent(new Event("input", { bubbles: true }));
}