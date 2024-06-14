interface ILabelProperties {
    label: string, 
    className: string | undefined,
    children: React.ReactNode,
}

export function LabelInput({label, className, children} : ILabelProperties) : React.ReactElement {
    return <label className={`form-control w-full ${className ?? ""}`}>
               <div className="label">{label}</div>
               {children}
           </label>;
}

export function ToolBarButton({title, onClick, children}: {title: string, onClick:React.MouseEvent<HTMLButtonElement>, children:any}) {
	return <button type="button" className="btn btn-accent btn-sm outline-none font-normal join-item" 
	               title={title}
	               onClick={onClick}>
		       {children ?? "err"}
	       </button>;
}