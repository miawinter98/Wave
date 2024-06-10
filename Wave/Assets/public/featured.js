const doInsertText = function(dataName, content) {
	const element = document.querySelector(`[data-wave-${dataName}]`);
	if (element) {
		if (element.tagName === "A") {
			element.href = content;
		} else if (element.tagName === "IMG") {
			element.src = content;
		} else {
			element.textContent = content;
		}
	}
}

const script = document.getElementById("wave-script");
if (!script) {
	throw new Error("[WAVE] no script with the id 'wave-script' exists.");
}
if (!script.src) {
	throw new Error("[WAVE] failed to get src attribute of element with id 'wave-script'.");
}
const scriptUrl = new URL(script.src);
const host = `${scriptUrl.protocol}//${scriptUrl.host}`;

let pfpSize = 150;
const container = document.querySelector("[data-wave]");

if (container && container.dataset.wavePfpSize) {
	const value = parseInt(container.dataset.wavePfpSize);
	if (value && value > 800) console.log("[WAVE] WARNING: pfp sizes greater 800 are not supported.");
	else if (value) pfpSize = value;
	else console.log(
		"[WAVE] WARNING: a custom pfp size has been provided with 'data-wave-pfp-size', " +
		"but it's value could not be parsed as an integer.");
}

console.log("[WAVE] requesting featured article");
fetch(new URL("/api/article/featured?size=" + pfpSize, host),
		{
			method: "GET",
			headers: new Headers({
				"Accept": "application/json"
			})
		})
	.then(response => response.json())
	.then(function(result) {

		if (container) {
			const template = document.querySelector("[data-wave-template]");

			if (template) {
				container.innerHTML = "";
				container.appendChild(template.content.cloneNode(true));
			} else {
				container.innerHTML = `
<div style="padding: 1em; border: 1px solid black; box-shadow: 4px 4px 0 0 currentColor; background: #ffb3c8;">
	<h1 data-wave-title style="margin: 0 0 0.5em 0"></h1>
	<img style="float: left; margin: 0 0.5em 0.5em 0; border: 1px solid transparent; border-radius: 0.25em" 
	     data-wave-author-profilePictureUrl alt="" width="${pfpSize}" />
	<p style="line-height: 1.4em; margin: 0">
		<a data-wave-author-profileUrl target="_blank" style="text-decoration: none; color: black">
			<small data-wave-author-name style="font-weight: bold"></small><br>
		</a>
		<small data-wave-publishDate></small><br>
		<span data-wave-contentPreview></span><br>
		<a data-wave-browserUrl target="_blank" style="color: black">Read More</a>
	</p>
	
</div>
`;
			}

			for (let [key, value] of Object.entries(result)) {
				doInsertText(key, value);
				if (typeof value === "object" && value != null) {
					for (let [innerKey, innerValue] of Object.entries(value)) {
						doInsertText(key + "-" + innerKey, innerValue);
					}
				}
			}

			console.log("[WAVE] fetched feature successfully.");
		} else {
			console.log("[WAVE] no container found, to use featured you require an element with the data tag 'wave'.");
		}
	})
	.catch(err => console.log(`[WAVE] failed to request featured article: ${err}.`));
