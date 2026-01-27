import { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';
import { getHomePageStats } from '../../services/homeApi';

const AnimatedStats = () => {
  const [isVisible, setIsVisible] = useState(false);
  const [stats, setStats] = useState([
    { value: 5000, suffix: '+', label: 'Events Catered', icon: '🎉' },
    { value: 500, suffix: '+', label: 'Catering Partners', icon: '🤝' },
    { value: 50000, suffix: '+', label: 'Happy Customers', icon: '😊' },
    { value: 98, suffix: '%', label: 'Satisfaction Rate', icon: '⭐' },
  ]);
  const [loading, setLoading] = useState(true);
  const sectionRef = useRef(null);

  useEffect(() => {
    const loadStats = async () => {
      try {
        const response = await getHomePageStats();
        if (response.success && response.data) {
          const apiData = response.data;
          setStats([
            { value: apiData.totalEventsCatered, suffix: '+', label: 'Events Catered', icon: '🎉' },
            { value: apiData.totalCateringPartners, suffix: '+', label: 'Catering Partners', icon: '🤝' },
            { value: apiData.totalHappyCustomers, suffix: '+', label: 'Happy Customers', icon: '😊' },
            { value: apiData.satisfactionRate, suffix: '%', label: 'Satisfaction Rate', icon: '⭐' },
          ]);
        }
      } catch (err) {
        console.error('Error loading homepage stats:', err);
        // Keep default stats on error
      } finally {
        setLoading(false);
      }
    };

    loadStats();
  }, []);

  useEffect(() => {
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
        }
      },
      { threshold: 0.3 }
    );

    if (sectionRef.current) {
      observer.observe(sectionRef.current);
    }

    return () => {
      if (sectionRef.current) {
        observer.unobserve(sectionRef.current);
      }
    };
  }, []);

  return (
    <section
      ref={sectionRef}
      className="py-20 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900 relative overflow-hidden"
    >
      {/* Elegant gold accent lines */}
      <div className="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-catering-accent to-transparent opacity-60"></div>
      <div className="absolute bottom-0 left-0 w-full h-1 bg-gradient-to-r from-transparent via-catering-accent to-transparent opacity-60"></div>

      {/* Decorative elements */}
      <div className="absolute top-1/2 left-10 w-32 h-32 bg-catering-accent/5 rounded-full blur-3xl"></div>
      <div className="absolute top-1/3 right-10 w-40 h-40 bg-catering-primary/5 rounded-full blur-3xl"></div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <motion.div
          initial={{ opacity: 0, y: 30 }}
          animate={isVisible ? { opacity: 1, y: 0 } : {}}
          transition={{ duration: 0.8 }}
          className="text-center mb-16"
        >
          <h2 className="text-4xl md:text-5xl font-bold text-white mb-4">
            Trusted by Thousands
          </h2>
          <div className="w-24 h-1 bg-gradient-to-r from-catering-accent to-catering-secondary mx-auto"></div>
        </motion.div>

        <div className="grid grid-cols-2 md:grid-cols-4 gap-8">
          {stats.map((stat, index) => (
            <StatCard
              key={index}
              stat={stat}
              index={index}
              isVisible={isVisible}
            />
          ))}
        </div>
      </div>
    </section>
  );
};

const StatCard = ({ stat, index, isVisible }) => {
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (!isVisible) return;

    const duration = 2000; // 2 seconds
    const steps = 60;
    const increment = stat.value / steps;
    let current = 0;

    const timer = setInterval(() => {
      current += increment;
      if (current >= stat.value) {
        setCount(stat.value);
        clearInterval(timer);
      } else {
        setCount(Math.floor(current));
      }
    }, duration / steps);

    return () => clearInterval(timer);
  }, [isVisible, stat.value]);

  return (
    <motion.div
      initial={{ opacity: 0, y: 50 }}
      animate={isVisible ? { opacity: 1, y: 0 } : {}}
      transition={{ duration: 0.6, delay: index * 0.15 }}
      className="text-center group"
    >
      <div className="bg-white/5 backdrop-blur-sm rounded-2xl p-8 border border-white/10 hover:border-catering-accent/50 transition-all duration-300 hover:shadow-2xl hover:shadow-catering-accent/20 hover:-translate-y-2">
        {/* Icon */}
        <div className="text-5xl mb-4 group-hover:scale-110 transition-transform duration-300">
          {stat.icon}
        </div>

        {/* Animated number */}
        <div className="text-5xl md:text-6xl font-bold mb-2 bg-gradient-to-br from-catering-accent via-white to-catering-secondary bg-clip-text text-transparent">
          {count.toLocaleString()}
          {stat.suffix}
        </div>

        {/* Label */}
        <div className="text-gray-300 text-sm md:text-base font-medium tracking-wide uppercase">
          {stat.label}
        </div>
      </div>
    </motion.div>
  );
};

export default AnimatedStats;
