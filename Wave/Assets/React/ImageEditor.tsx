import React, {ChangeEvent, useEffect, useState} from "react";
import Modal from "./Modal";

interface ImageEditorProperties {
    open: boolean,
    onClose: () => void,
    callback: (location: string, description?: string) => void,
    t: any
}

const ImageEditor = function({open = false, onClose, callback, t}: ImageEditorProperties){
    const [busy, setBusy] = useState(false);
    const [file, setFile] = useState("");

    async function onSubmit(event: React.FormEvent) {
        event.preventDefault();
        const elem = event.target as HTMLFormElement;
        const fileElem = elem.file as HTMLInputElement;
        if (fileElem.value.length < 1) return;

        if (busy) return;
        setBusy(true);

        try {
            const formData = new FormData(elem);
            let response = await fetch("/images/create", {
                method: "PUT",
                body: formData
            })
            if (!response.ok) {
                throw new Error(response.statusText);
            }

            (event.target as HTMLFormElement)?.reset()
            setFile("")

            const loc = response.headers.get("Location") as string;
            callback(loc, formData.get("imageAlt") as string);
        } finally {
            setBusy(false);
        }
    }

    function onImageChange(event: ChangeEvent) {
        const fileInput = event.target as HTMLInputElement;
        if (!fileInput || !fileInput.files || fileInput.files.length < 1) return;
        setFile(URL.createObjectURL(fileInput.files[0]));
    }

    return (
        <Modal open={open} onClose={onClose} t={t}>
            <div className="grid grid-rows-1 md:grid-rows-1 md:grid-cols-2 gap-2 max-w-2xl">
                <form onSubmit={onSubmit} className="flex flex-col justify-stretch items-center gap-2 order-2">
                    <input id="file" name="file" type="file" alt="Image" onChange={onImageChange}
                           className="file-input file-input-bordered w-full max-w-xs" autoFocus={true}/>

                    <label className="form-control w-full max-w-xs">
                        <div className="label">
                            <span className="label-text">{t("image.Quality")}</span>
                        </div>
                        <select id="quality" name="quality" className="select select-bordered w-full">
                            <option value={0}>{t("image.quality.Normal")}</option>
                            <option value={1}>{t("image.quality.High")}</option>
                            <option value={2}>{t("image.quality.Source")}</option>
                        </select>
                    </label>

                    <label className="form-control w-full max-w-xs">
                        <div className="label">
                            <span className="label-text">{t("image.Alt")}</span>
                        </div>
                        <input type="text" name="imageAlt" className="input input-bordered w-full" autoComplete={"off"} />
                    </label>

                    <button type="submit" className="btn btn-primary w-full max-w-xs" disabled={busy}>
                        {t("image.Save")}
                    </button>

                    {busy &&
                        <div className="w-full flex gap-2 items-center justify-center">
                            <span className="loading loading-spinner loading-lg"></span>
                            {t("image.Uploading")}
                        </div>
                    }
                </form>
                <figure className="border-2 bg-base-300 border-primary rounded-lg overflow-hidden order-1 md:order-3">
                    <img className="w-full object-center object-contain" src={file}
                         alt=""/>
                </figure>
            </div>
        </Modal>
    )
}

export default ImageEditor;