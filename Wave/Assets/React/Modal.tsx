import React, {useEffect, useRef} from "react";

interface ModalProperties {
    open: boolean,
    onClose: () => void,
    className?: string,
    children: React.ReactNode,
    t: any,
}

const Modal = function({open, onClose, children, t}: ModalProperties) {
    const ref = useRef<HTMLDialogElement>(null);

    useEffect(() => {
        if (open && ref.current) ref.current.showModal();
        else if (ref.current) ref.current.close()
    }, [open])

    return (
        <dialog ref={ref} onCancel={onClose} id="test"
                className="p-4 rounded-lg bg-base-200 text-base-content border border-base-300 shadow z-[100] backdrop:bg-base-100 backdrop:bg-opacity-50">
            <div className="flex flex-col gap-2 sm:min-w-80">
                {children}

                <button type="button" className="btn btn-error self-end" onClick={onClose}>
                    {t("dialog.Cancel")}
                </button>
            </div>
        </dialog>
    )
}

export default Modal;