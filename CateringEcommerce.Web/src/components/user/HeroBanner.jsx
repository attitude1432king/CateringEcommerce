import React, { useRef, useEffect, useState, useCallback } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { MapPin, Search, Star, Users, CheckCircle, ChevronRight } from 'lucide-react';

export default function HeroBanner({ onSearch }) {
    const locationInputRef  = useRef(null);
    const searchFieldRef    = useRef(null); // wraps the search input + its dropdown
    const [locationTerm,    setLocationTerm]    = useState('');
    const [cateringSearch,  setCateringSearch]  = useState('');
    const [showSuggestions, setShowSuggestions] = useState(false);
    const [activeIndex,     setActiveIndex]     = useState(-1);

    const popularSearches = [
        { term: 'Wedding Catering',  icon: '💒', description: 'Elegant wedding services' },
        { term: 'Corporate Events',  icon: '🏢', description: 'Professional business catering' },
        { term: 'Birthday Parties',  icon: '🎂', description: 'Celebrate with delicious food' },
        { term: 'Indian Cuisine',    icon: '🍛', description: 'Authentic Indian flavors' },
        { term: 'BBQ & Grill',       icon: '🍖', description: 'Grilled specialties' },
        { term: 'Vegan Options',     icon: '🥗', description: 'Plant-based catering' },
    ];

    const filteredSuggestions = cateringSearch
        ? popularSearches.filter(s => s.term.toLowerCase().includes(cateringSearch.toLowerCase()))
        : popularSearches;

    /* ── Google Maps autocomplete on location field ── */
    useEffect(() => {
        if (window.google?.maps?.places && locationInputRef.current) {
            try {
                const ac = new window.google.maps.places.Autocomplete(locationInputRef.current, { types: ['geocode'] });
                ac.setFields(['formatted_address', 'geometry', 'place_id']);
                ac.addListener('place_changed', () => {
                    const place = ac.getPlace();
                    if (onSearch) onSearch({ address: place.formatted_address, cateringSearch, place });
                });
            } catch (_) { /* Google Maps not loaded */ }
        }
    }, [onSearch, cateringSearch]);

    /* ── Close on outside click ── */
    useEffect(() => {
        const handler = (e) => {
            if (searchFieldRef.current && !searchFieldRef.current.contains(e.target)) {
                setShowSuggestions(false);
                setActiveIndex(-1);
            }
        };
        document.addEventListener('mousedown', handler);
        return () => document.removeEventListener('mousedown', handler);
    }, []);

    /* ── Keyboard navigation ── */
    const handleKeyDown = useCallback((e) => {
        if (!showSuggestions) return;
        if (e.key === 'ArrowDown') {
            e.preventDefault();
            setActiveIndex(i => Math.min(i + 1, filteredSuggestions.length - 1));
        } else if (e.key === 'ArrowUp') {
            e.preventDefault();
            setActiveIndex(i => Math.max(i - 1, -1));
        } else if (e.key === 'Enter' && activeIndex >= 0) {
            e.preventDefault();
            handleSuggestionClick(filteredSuggestions[activeIndex]);
        } else if (e.key === 'Escape') {
            setShowSuggestions(false);
            setActiveIndex(-1);
        }
    }, [showSuggestions, activeIndex, filteredSuggestions]);

    const handleSubmit = (e) => {
        e.preventDefault();
        if (onSearch) onSearch({ location: locationTerm, cateringSearch });
        setShowSuggestions(false);
        setActiveIndex(-1);
    };

    const handleSuggestionClick = (suggestion) => {
        setCateringSearch(suggestion.term);
        setShowSuggestions(false);
        setActiveIndex(-1);
        if (onSearch) onSearch({ location: locationTerm, cateringSearch: suggestion.term });
    };

    const trustItems = [
        { Icon: CheckCircle, value: '500+',  label: 'Verified Caterers' },
        { Icon: Star,        value: '4.8/5', label: 'Average Rating'    },
        { Icon: Users,       value: '50k+',  label: 'Happy Clients'     },
    ];

    return (
        <section className="hero">
            {/* Background layer — has its own overflow:hidden so video stays clipped
                while the hero section itself stays overflow:visible for the dropdown */}
            <div className="hero__bg-layer" aria-hidden="true">
                <video autoPlay muted loop playsInline preload="metadata">
                    <source src="/catering-hero.mp4" type="video/mp4" />
                </video>
                <div className="hero__veil" />
            </div>

            {/* Content */}
            <div className="hero__content">
                <motion.div
                    initial={{ opacity: 0, y: -16 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.7 }}
                >
                    <div className="hero__eyebrow">
                        <Star size={13} className="star" fill="currentColor" />
                        <span>PREMIUM CATERING SERVICES</span>
                    </div>
                </motion.div>

                <motion.h1
                    className="hero__title"
                    initial={{ opacity: 0, y: 24 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.7, delay: 0.15 }}
                >
                    Exquisite Catering<br />
                    <span className="hero__title-grad">for Every Occasion</span>
                </motion.h1>

                <motion.p
                    className="hero__sub"
                    initial={{ opacity: 0, y: 24 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.7, delay: 0.28 }}
                >
                    Elevate your weddings, corporate events, and celebrations with{' '}
                    <em>verified premium caterers</em>
                </motion.p>

                {/* Search bar */}
                <motion.form
                    onSubmit={handleSubmit}
                    initial={{ opacity: 0, y: 24 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.7, delay: 0.42 }}
                >
                    <div className="hero__search">
                        {/* Location field */}
                        <div className="hero__field">
                            <MapPin size={16} />
                            <input
                                ref={locationInputRef}
                                type="text"
                                placeholder="Enter your city"
                                value={locationTerm}
                                onChange={(e) => setLocationTerm(e.target.value)}
                                aria-label="Location"
                            />
                        </div>

                        {/*
                          Catering search field — this div is the ONLY positioning
                          context for the dropdown. `position: relative` comes from
                          .hero__field in enyvora.css. The dropdown uses
                          `left:0; right:0` so it matches THIS field's width exactly.
                        */}
                        <div className="hero__field" ref={searchFieldRef}>
                            <Search size={16} />
                            <input
                                type="text"
                                placeholder="Search catering services"
                                value={cateringSearch}
                                onChange={(e) => {
                                    setCateringSearch(e.target.value);
                                    setShowSuggestions(true);
                                    setActiveIndex(-1);
                                }}
                                onFocus={() => setShowSuggestions(true)}
                                onKeyDown={handleKeyDown}
                                aria-label="Catering search"
                                aria-autocomplete="list"
                                aria-expanded={showSuggestions}
                                autoComplete="off"
                            />

                            {/* Dropdown — width = .hero__field width (flex:1 child of .hero__search) */}
                            <AnimatePresence>
                                {showSuggestions && filteredSuggestions.length > 0 && (
                                    <motion.div
                                        key="hero-suggestions"
                                        initial={{ opacity: 0, y: -6, scale: 0.98 }}
                                        animate={{ opacity: 1, y: 0,  scale: 1    }}
                                        exit={{    opacity: 0, y: -6, scale: 0.98 }}
                                        transition={{ type: 'spring', stiffness: 400, damping: 28 }}
                                        role="listbox"
                                        aria-label="Search suggestions"
                                        style={{
                                            position:     'absolute',
                                            top:          'calc(100% + 10px)',
                                            left:         0,
                                            right:        0,
                                            zIndex:       9999,
                                            background:   '#fff',
                                            borderRadius: '16px',
                                            boxShadow:    '0 20px 60px rgba(0,0,0,0.20), 0 4px 16px rgba(0,0,0,0.08)',
                                            border:       '1px solid rgba(0,0,0,0.07)',
                                            overflow:     'hidden',
                                            maxHeight:    '300px',
                                            overflowY:    'auto',
                                        }}
                                    >
                                        {/* Section label */}
                                        <div style={{
                                            padding:      '10px 14px 6px',
                                            borderBottom: '1px solid #f3f4f6',
                                        }}>
                                            <span style={{
                                                fontSize:      '10px',
                                                fontWeight:    700,
                                                color:         '#9ca3af',
                                                textTransform: 'uppercase',
                                                letterSpacing: '0.1em',
                                            }}>
                                                Popular Searches
                                            </span>
                                        </div>

                                        {/* Suggestion items */}
                                        <div style={{ padding: '6px 8px 8px' }}>
                                            {filteredSuggestions.map((s, i) => (
                                                <button
                                                    key={s.term}
                                                    type="button"
                                                    role="option"
                                                    aria-selected={i === activeIndex}
                                                    onMouseEnter={() => setActiveIndex(i)}
                                                    onClick={() => handleSuggestionClick(s)}
                                                    style={{
                                                        width:          '100%',
                                                        display:        'flex',
                                                        alignItems:     'center',
                                                        gap:            '10px',
                                                        padding:        '9px 10px',
                                                        borderRadius:   '10px',
                                                        border:         'none',
                                                        cursor:         'pointer',
                                                        textAlign:      'left',
                                                        transition:     'background 0.12s',
                                                        background:     i === activeIndex ? 'rgba(255,107,53,0.06)' : 'transparent',
                                                        color:          'inherit',
                                                    }}
                                                >
                                                    <span style={{ fontSize: '20px', flexShrink: 0, lineHeight: 1 }}>
                                                        {s.icon}
                                                    </span>
                                                    <div style={{ flex: 1, minWidth: 0 }}>
                                                        <div style={{
                                                            fontSize:     '13px',
                                                            fontWeight:   600,
                                                            color:        i === activeIndex ? 'var(--color-primary)' : '#111827',
                                                            overflow:     'hidden',
                                                            textOverflow: 'ellipsis',
                                                            whiteSpace:   'nowrap',
                                                            transition:   'color 0.12s',
                                                        }}>
                                                            {s.term}
                                                        </div>
                                                        <div style={{
                                                            fontSize:     '11px',
                                                            color:        '#9ca3af',
                                                            marginTop:    '1px',
                                                            overflow:     'hidden',
                                                            textOverflow: 'ellipsis',
                                                            whiteSpace:   'nowrap',
                                                        }}>
                                                            {s.description}
                                                        </div>
                                                    </div>
                                                    <ChevronRight
                                                        size={13}
                                                        style={{
                                                            color:      i === activeIndex ? 'var(--color-accent)' : '#d1d5db',
                                                            flexShrink: 0,
                                                            transition: 'color 0.12s',
                                                        }}
                                                    />
                                                </button>
                                            ))}
                                        </div>
                                    </motion.div>
                                )}
                            </AnimatePresence>
                        </div>

                        {/* CTA */}
                        <button
                            type="submit"
                            className="hero__cta"
                            style={{ background: 'linear-gradient(135deg, #FF6B35 0%, #FF8C42 50%, #FFB627 100%)', color: '#fff' }}
                        >
                            <Search size={16} />
                            <span>Find Caterers</span>
                        </button>
                    </div>
                </motion.form>

                {/* Trust strip */}
                <motion.div
                    className="hero__trust"
                    initial={{ opacity: 0, y: 24 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.7, delay: 0.56 }}
                >
                    {trustItems.map(({ Icon, value, label }) => (
                        <div key={label} className="trust">
                            <div className="trust__ic">
                                <Icon size={20} strokeWidth={2} />
                            </div>
                            <div>
                                <div className="trust__n">{value}</div>
                                <div className="trust__l">{label}</div>
                            </div>
                        </div>
                    ))}
                </motion.div>
            </div>
        </section>
    );
}
