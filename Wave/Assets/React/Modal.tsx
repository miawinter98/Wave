import React, {useEffect, useRef} from "react";

interface ModalProperties {
    open: boolean,
    onClose: () => void,
    className?: string,
    children: React.ReactNode,
}

const Modal = function({open, onClose, children}: ModalProperties) {
    const ref = useRef<HTMLDialogElement>(null);

    useEffect(() => {
        if (open && ref.current) ref.current.showModal();
        else if (ref.current) ref.current.close()
    }, [open])

    return (
        <dialog ref={ref} onCancel={onClose} className="p-4 rounded-lg bg-base-200 border border-base-300 shadow z-[100] backdrop:bg-base-100 backdrop:bg-opacity-50">
            {children}

            <button type="button" className="btn btn-primary w-full sm:btn-wide" onClick={onClose}>
                Cancel
            </button>
        </dialog>
    )
}

export default Modal;