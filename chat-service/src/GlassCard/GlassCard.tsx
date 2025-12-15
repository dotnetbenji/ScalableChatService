import type { ReactNode } from "react";
import "./GlassCard.css";

export interface GlassCardProps {
    children: ReactNode;
    className?: string;
    onClick?: () => void;
}

const GlassCard = ({
    children,
    className = "",
    onClick,
}: GlassCardProps) => {
    return (
        <div
            className={`glass-card ${className}`}
            onClick={onClick}
        >
            {children}
        </div>
    );
};

export default GlassCard;
