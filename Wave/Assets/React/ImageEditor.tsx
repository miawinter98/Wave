import React, {useEffect} from "react";
import Modal from "./Modal";

const ImageEditor = function({open = false, onClose, callback}: {open: boolean, onClose: () => void, callback: (location: string) => void}){
    async function onSubmit(event: React.FormEvent) {
        event.preventDefault();

        const formData = new FormData(event.target as HTMLFormElement);
        let response = await fetch("/images/create", {
            method: "PUT",
            body: formData
        })
        if (!response.ok) {
            throw new Error(response.statusText);
        }

        (event.target as HTMLFormElement)?.reset()

        const loc = response.headers.get("Location") as string;
        callback(loc);
    }

    return (
        <Modal open={open} onClose={onClose}>
            <form onSubmit={onSubmit} className="flex gap-2 p-2">
                <input id="file" name="file" type="file" alt="Image"
                       className="file-input file-input-bordered w-full max-w-xs" autoFocus={true}/>

                <button type="submit" className="btn btn-primary w-full sm:btn-wide">Upload</button>
            </form>
        </Modal>
    )
}

export default ImageEditor;