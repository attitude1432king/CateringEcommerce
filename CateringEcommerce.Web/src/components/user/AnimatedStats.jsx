import { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';
import { PartyPopper, Handshake, Smile, Star } from 'lucide-react';
import { getHomePageStats } from '../../services/homeApi';

const DEFAULT_STATS = [
    { value: 5000,  suffix: '+', label: 'Events Catered',     Icon: PartyPopper },
    { value: 500,   suffix: '+', label: 'Catering Partners',  Icon: Handshake   },
    { value: 50000, suffix: '+', label: 'Happy Customers',    Icon: Smile       },
    { value: 98,    suffix: '%', label: 'Satisfaction Rate',  Icon: Star        },
];

const StatCard = ({ stat, index, isVisible }) => {
    const [count, setCount] = useState(0);

    useEffect(() => {
        if (!isVisible) return;
        const duration = 2000;
        const steps = 60;
        const increment = stat.value / steps;
        let current = 0;
        const timer = setInterval(() => {
            current += increment;
            if (current >= stat.value) { setCount(stat.value); clearInterval(timer); }
            else setCount(Math.floor(current));
        }, duration / steps);
        return () => clearInterval(timer);
    }, [isVisible, stat.value]);

    return (
        <motion.div
            initial={{ opacity: 0, y: 40 }}
            animate={isVisible ? { opacity: 1, y: 0 } : {}}
            transition={{ duration: 0.55, delay: index * 0.12 }}
            className="text-center group"
        >
            <div className="bg-white/8 backdrop-blur-sm rounded-2xl p-8 border border-white/10 hover:border-accent/50 transition-all duration-300 hover:shadow-[0_20px_60px_rgba(255,182,39,0.15)] hover:-translate-y-2">
                <div className="w-12 h-12 rounded-2xl mx-auto mb-5 flex items-center justify-center group-hover:scale-110 transition-transform duration-300"
                    style={{ background: 'rgba(255,182,39,0.15)', color: 'var(--color-accent)' }}>
                    <stat.Icon size={24} strokeWidth={2} />
                </div>
                <div
                    className="text-5xl md:text-6xl font-extrabold mb-2 tabular-nums"
                    style={{ background: 'linear-gradient(135deg, var(--color-accent), #fff, var(--color-secondary))', WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent', backgroundClip: 'text' }}
                >
                    {count.toLocaleString()}{stat.suffix}
                </div>
                <div className="text-neutral-400 text-sm font-medium uppercase tracking-widest">{stat.label}</div>
            </div>
        </motion.div>
    );
};

export default function AnimatedStats() {
    const sectionRef = useRef(null);
    const [isVisible, setIsVisible] = useState(false);
    const [stats, setStats] = useState(DEFAULT_STATS);

    useEffect(() => {
        const loadStats = async () => {
            try {
                const response = await getHomePageStats();
                if (response.success && response.data) {
                    const d = response.data;
                    setStats([
                        { value: d.totalEventsCatered,    suffix: '+', label: 'Events Catered',    Icon: PartyPopper },
                        { value: d.totalCateringPartners, suffix: '+', label: 'Catering Partners', Icon: Handshake   },
                        { value: d.totalHappyCustomers,   suffix: '+', label: 'Happy Customers',   Icon: Smile       },
                        { value: d.satisfactionRate,      suffix: '%', label: 'Satisfaction Rate', Icon: Star        },
                    ]);
                }
            } catch {/* keep defaults */}
        };
        loadStats();
    }, []);

    useEffect(() => {
        const observer = new IntersectionObserver(([entry]) => { if (entry.isIntersecting) setIsVisible(true); }, { threshold: 0.3 });
        if (sectionRef.current) observer.observe(sectionRef.current);
        return () => { if (sectionRef.current) observer.unobserve(sectionRef.current); };
    }, []);

    return (
        <section ref={sectionRef} className="py-20 relative overflow-hidden" style={{ background: 'linear-gradient(135deg, #111827, #1f2937)' }}>
            {/* Accent lines */}
            <div className="absolute top-0 left-0 w-full h-px" style={{ background: 'linear-gradient(90deg, transparent, var(--color-accent), transparent)' }} />
            <div className="absolute bottom-0 left-0 w-full h-px" style={{ background: 'linear-gradient(90deg, transparent, var(--color-accent), transparent)' }} />

            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <motion.div
                    initial={{ opacity: 0, y: 24 }}
                    animate={isVisible ? { opacity: 1, y: 0 } : {}}
                    transition={{ duration: 0.6 }}
                    className="text-center mb-14"
                >
                    <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
                        Trusted by Thousands
                    </h2>
                    <div className="w-24 h-1 mx-auto rounded-full" style={{ background: 'var(--gradient-catering)' }} />
                </motion.div>

                <div className="grid grid-cols-2 md:grid-cols-4 gap-6">
                    {stats.map((stat, index) => (
                        <StatCard key={index} stat={stat} index={index} isVisible={isVisible} />
                    ))}
                </div>
            </div>
        </section>
    );
}
