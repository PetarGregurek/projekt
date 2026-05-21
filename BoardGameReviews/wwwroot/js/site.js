const debounce = (callback, delay = 250) => {
	let timerId;

	return (...args) => {
		window.clearTimeout(timerId);
		timerId = window.setTimeout(() => callback(...args), delay);
	};
};

const wireAjaxSearch = () => {
	document.querySelectorAll('[data-ajax-search]').forEach((input) => {
		if (input.dataset.bound === 'true') {
			return;
		}

		input.dataset.bound = 'true';
		const targetSelector = input.dataset.searchTarget;
		const searchUrl = input.dataset.searchUrl;
		const target = document.querySelector(targetSelector);

		if (!target || !searchUrl) {
			return;
		}

		const runSearch = debounce(async () => {
			const url = new URL(searchUrl, window.location.origin);
			url.searchParams.set('search', input.value);

			const response = await fetch(url, {
				headers: {
					'X-Requested-With': 'XMLHttpRequest'
				}
			});

			if (!response.ok) {
				return;
			}

			target.innerHTML = await response.text();
		});

		input.addEventListener('input', runSearch);
	});
};

const shakeField = (field) => {
	if (!field) {
		return;
	}

	field.classList.remove('field-shake');
	void field.offsetWidth;
	field.classList.add('field-shake');
};

const getValidationMessageSpan = (form, fieldName) =>
	form.querySelector(`[data-valmsg-for="${fieldName}"]`);

const setValidationMessage = (form, fieldName, message) => {
	const span = getValidationMessageSpan(form, fieldName);
	if (!span) {
		return;
	}

	span.textContent = message;
	span.classList.remove('field-validation-valid');
	span.classList.add('field-validation-error');
	span.dataset.customValidation = 'true';
};

const clearValidationMessage = (form, fieldName) => {
	const span = getValidationMessageSpan(form, fieldName);
	if (!span || span.dataset.customValidation !== 'true') {
		return;
	}

	span.textContent = '';
	span.classList.remove('field-validation-error');
	span.classList.add('field-validation-valid');
	delete span.dataset.customValidation;
};

const validatePlayersRelation = (form, animate = true) => {
	const minField = form.querySelector('[name="Input.MinPlayers"]');
	const maxField = form.querySelector('[name="Input.MaxPlayers"]');

	if (!minField || !maxField) {
		return true;
	}

	const minRaw = (minField.value || '').trim();
	const maxRaw = (maxField.value || '').trim();

	if (minRaw.length === 0 || maxRaw.length === 0) {
		clearValidationMessage(form, 'Input.MaxPlayers');
		return true;
	}

	const minValue = Number(minRaw);
	const maxValue = Number(maxRaw);
	if (!Number.isFinite(minValue) || !Number.isFinite(maxValue)) {
		return true;
	}

	if (maxValue < minValue) {
		if (animate) {
			applyFieldState(maxField, 'invalid');
		}

		setValidationMessage(form, 'Input.MaxPlayers', 'Maximum players must be greater than or equal to minimum players.');
		return false;
	}

	clearValidationMessage(form, 'Input.MaxPlayers');
	if (animate && !maxField.classList.contains('field-missing')) {
		applyFieldState(maxField, 'valid');
	}

	return true;
};

const markJQueryErrors = (form) => {
	if (!window.jQuery || !window.jQuery(form).data('validator')) {
		return true;
	}

	const validator = window.jQuery(form).data('validator');
	let valid = true;

	validator.errorList.forEach((error) => {
		valid = false;
		const element = error.element;
		applyFieldState(element, 'invalid');
	});

	return valid;
};

const applyFieldState = (field, state) => {
	field.classList.remove('field-missing');
	if (state === 'missing' || state === 'invalid') {
		field.classList.add('field-missing');
		shakeField(field);
		return false;
	}
	return true;
};

const validateNativeField = (field) => {
	if (!field || field.type === 'hidden' || field.disabled || field.type === 'checkbox') {
		return true;
	}

	const form = field.closest('form');
	const hasJq = form && window.jQuery && window.jQuery(form).data('validator');
	const value = (field.value || '').trim();
	const hasRequiredRule = field.required || field.dataset.valRequired !== undefined;
	const isMissing = hasRequiredRule && value.length === 0;

	if (isMissing) {
		if (!hasJq && form) {
			setValidationMessage(form, field.name, field.dataset.valRequired || 'This field is required.');
		}
		return applyFieldState(field, 'missing');
	}

	if (value.length > 0) {
		const minValue = field.dataset.valRangeMin;
		const maxValue = field.dataset.valRangeMax;
		if (minValue !== undefined && maxValue !== undefined) {
			const numericValue = Number(value);
			const min = Number(minValue);
			const max = Number(maxValue);
			if (!Number.isFinite(numericValue) || numericValue < min || numericValue > max) {
				if (!hasJq && form) {
					const msg = field.dataset.valRange || `Must be between ${min} and ${max}.`;
					setValidationMessage(form, field.name, msg);
				}
				return applyFieldState(field, 'invalid');
			}
		}

		if (!field.checkValidity()) {
			if (!hasJq && form) {
				const msg = field.dataset.valEmail || field.dataset.valRegex || 'Invalid value.';
				setValidationMessage(form, field.name, msg);
			}
			return applyFieldState(field, 'invalid');
		}
	}

	if (!hasJq && form) clearValidationMessage(form, field.name);
	return applyFieldState(field, 'valid');
};

const wireFieldFeedback = () => {
	document.querySelectorAll('form').forEach((form) => {
		if (form.dataset.feedbackBound === 'true') {
			return;
		}

		form.dataset.feedbackBound = 'true';

		form.querySelectorAll('input, select, textarea').forEach((field) => {
			if (field.type === 'hidden') {
				return;
			}

			field.addEventListener('blur', () => {
				validateNativeField(field);
				if (field.name === 'Input.MinPlayers' || field.name === 'Input.MaxPlayers') {
					validatePlayersRelation(form, true);
				}
			});

			field.addEventListener('input', () => {
				if ((field.value || '').trim().length > 0) {
					field.classList.remove('field-missing');
				}

				if (field.name === 'Input.MinPlayers' || field.name === 'Input.MaxPlayers') {
					clearValidationMessage(form, 'Input.MaxPlayers');
				}
			});
		});
	});
};

const createOptionMarkup = (item) => `
	<button type="button" class="autocomplete-dropdown__option" data-autocomplete-option data-value="${item.id}" data-label="${item.label}">
		<span class="autocomplete-dropdown__option-title">${item.label}</span>
		<span class="autocomplete-dropdown__option-meta">${item.description ?? ''}</span>
	</button>`;

const wireAutocompleteDropdowns = () => {
	document.querySelectorAll('[data-autocomplete-dropdown]').forEach((root) => {
		if (root.dataset.bound === 'true') {
			return;
		}

		root.dataset.bound = 'true';
		const lookupUrl = root.dataset.lookupUrl;
		const hiddenInput = root.querySelector('[data-autocomplete-value]');
		const textInput = root.querySelector('[data-autocomplete-input]');
		const panel = root.querySelector('[data-autocomplete-panel]');
		const results = root.querySelector('[data-autocomplete-results]');
		const validation = root.querySelector('[data-autocomplete-validation]');
		const requiredMessage = hiddenInput?.dataset.requiredMessage || 'Please select an option.';
		const invalidSelectionMessage = 'Please select an option from the list.';
		let hasInteracted = false;

		if (!lookupUrl || !hiddenInput || !textInput || !panel || !results || !validation) {
			return;
		}

		const setValidationState = (state) => {
			if (state === 'valid') {
				validation.textContent = '';
				validation.classList.add('autocomplete-dropdown__validation--hidden');
				applyFieldState(textInput, 'valid');
				return true;
			}

			validation.textContent = state === 'missing' ? requiredMessage : invalidSelectionMessage;
			validation.classList.remove('autocomplete-dropdown__validation--hidden');
			applyFieldState(textInput, state);
			return false;
		};

		const validateSelection = () => {
			const selectedValue = Number.parseInt(hiddenInput.value || '0', 10);
			if (Number.isFinite(selectedValue) && selectedValue > 0) {
				return setValidationState('valid');
			}

			const hasTypedValue = (textInput.value || '').trim().length > 0;
			return setValidationState(hasTypedValue ? 'invalid' : 'missing');
		};

		const renderResults = (items) => {
			if (!items.length) {
				results.innerHTML = '<div class="autocomplete-dropdown__empty">No results found.</div>';
				panel.hidden = false;
				return;
			}

			results.innerHTML = items.map(createOptionMarkup).join('');
			panel.hidden = false;
		};

		const loadResults = debounce(async () => {
			const url = new URL(lookupUrl, window.location.origin);
			url.searchParams.set('query', textInput.value);

			const response = await fetch(url);
			if (!response.ok) {
				return;
			}

			const items = await response.json();
			renderResults(items);
			textInput.setAttribute('aria-expanded', 'true');
		}, 200);

		textInput.addEventListener('focus', loadResults);
		textInput.addEventListener('focus', () => {
			hasInteracted = true;
		});
		textInput.addEventListener('input', () => {
			hasInteracted = true;
			hiddenInput.value = '0';
			validation.classList.add('autocomplete-dropdown__validation--hidden');
			textInput.classList.remove('field-missing');
			loadResults();
		});

		textInput.addEventListener('blur', () => {
			window.setTimeout(() => {
				if (!panel.hidden) {
					return;
				}

				if (hasInteracted || (textInput.value || '').trim().length > 0) {
					validateSelection();
				}
			}, 120);
		});

		results.addEventListener('click', (event) => {
			const option = event.target.closest('[data-autocomplete-option]');
			if (!option) {
				return;
			}

			hiddenInput.value = option.dataset.value;
			textInput.value = option.dataset.label;
			panel.hidden = true;
			textInput.setAttribute('aria-expanded', 'false');
			setValidationState('valid');
		});

		document.addEventListener('click', (event) => {
			if (root.contains(event.target)) {
				return;
			}

			panel.hidden = true;
			textInput.setAttribute('aria-expanded', 'false');
		});

		const form = root.closest('form');
		if (form) {
			if (!form._autocompleteValidators) {
				form._autocompleteValidators = [];
			}

			form._autocompleteValidators.push(validateSelection);
		}
	});
};

const wireDateTimePickers = () => {
	const locale = ((navigator.languages && navigator.languages[0]) || navigator.language || 'hr').toLowerCase();
	const isHr = locale.startsWith('hr');
	const requiredDateMessage = isHr ? 'Odaberite datum i vrijeme.' : 'Select date and time.';
	const partialMessage = isHr ? 'Odaberite i datum i vrijeme.' : 'Select both date and time.';
	const invalidMessage = isHr ? 'Neispravna vrijednost datuma i vremena.' : 'Invalid date/time value.';
	const invalidMinuteMessage = isHr ? 'Minute moraju biti između 00 i 59.' : 'Minutes must be between 00 and 59.';
	const invalidHourMessage = isHr ? 'Sati moraju biti između 1 i 12.' : 'Hours must be between 1 and 12.';
	const invalidYearMessage = isHr ? 'Godina mora biti između 1900 i 2100.' : 'Year must be between 1900 and 2100.';
	const invalidDayMessage = isHr ? 'Dan mora biti valjan za odabrani mjesec.' : 'Day must be valid for selected month.';
	const hoursLabel = isHr ? 'Sati' : 'Hours';
	const minutesLabel = isHr ? 'Minute' : 'Minutes';
	const selectDateText = isHr ? 'Odaberite datum' : 'Select date';
	const selectTimeText = isHr ? 'Odaberite vrijeme' : 'Select time';

	const monthFormatter = new Intl.DateTimeFormat(locale, { month: 'long', year: 'numeric' });
	const dayFormatter = new Intl.DateTimeFormat(locale, { day: '2-digit', month: '2-digit', year: 'numeric' });
	const summaryFormatter = new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' });
	const weekdayFormatter = new Intl.DateTimeFormat(locale, { weekday: 'short' });
	const weekdaySeed = isHr ? [1, 2, 3, 4, 5, 6, 7] : [7, 1, 2, 3, 4, 5, 6];
	const weekdayLabels = weekdaySeed.map((day) => weekdayFormatter.format(new Date(2024, 0, day)));

	const parseISO = (iso) => {
		if (!iso) {
			return null;
		}

		const match = iso.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/);
		if (!match) {
			return null;
		}

		const year = Number(match[1]);
		const month = Number(match[2]);
		const day = Number(match[3]);
		const hour = Number(match[4]);
		const minute = Number(match[5]);
		const parsed = new Date(year, month - 1, day, hour, minute, 0, 0);

		if (Number.isNaN(parsed.getTime()) || parsed.getFullYear() !== year || parsed.getMonth() + 1 !== month || parsed.getDate() !== day) {
			return null;
		}

		return parsed;
	};

	const toISO = (date) =>
		`${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}T${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;

	const clampMonth = (year, month) => {
		if (month < 0) {
			return { year: year - 1, month: 11 };
		}

		if (month > 11) {
			return { year: year + 1, month: 0 };
		}

		return { year, month };
	};

	const getHour12 = (hour24) => (hour24 % 12) || 12;

	document.querySelectorAll('[data-dtp]').forEach((root) => {
		if (root.dataset.dtpBound === 'true') {
			return;
		}

		root.dataset.dtpBound = 'true';

		const hidden = root.querySelector('[data-dtp-hidden]');
		const trigger = root.querySelector('[data-dtp-trigger]');
		const summary = root.querySelector('[data-dtp-summary]');
		const panel = root.querySelector('[data-dtp-panel]');
		const validation = root.querySelector('[data-dtp-validation]');
		const monthLabel = root.querySelector('[data-dtp-month-label]');
		const yearMinusButton = root.querySelector('[data-dtp-year-minus]');
		const yearPlusButton = root.querySelector('[data-dtp-year-plus]');
		const yearInput = root.querySelector('[data-dtp-year-input]');
		const dayMinusButton = root.querySelector('[data-dtp-day-minus]');
		const dayPlusButton = root.querySelector('[data-dtp-day-plus]');
		const dayInput = root.querySelector('[data-dtp-day-input]');
		const weekdays = root.querySelector('[data-dtp-weekdays]');
		const daysGrid = root.querySelector('[data-dtp-days]');
		const prevButton = root.querySelector('[data-dtp-prev]');
		const nextButton = root.querySelector('[data-dtp-next]');
		const hourClock = root.querySelector('[data-dtp-hour-clock]');
		const minuteClock = root.querySelector('[data-dtp-minute-clock]');
		const hourMinusButton = root.querySelector('[data-dtp-hour-minus]');
		const hourPlusButton = root.querySelector('[data-dtp-hour-plus]');
		const hourInput = root.querySelector('[data-dtp-hour-input]');
		const minuteMinusButton = root.querySelector('[data-dtp-minute-minus]');
		const minutePlusButton = root.querySelector('[data-dtp-minute-plus]');
		const minuteInput = root.querySelector('[data-dtp-minute-input]');
		const amButton = root.querySelector('[data-dtp-am]');
		const pmButton = root.querySelector('[data-dtp-pm]');
		const todayButton = root.querySelector('[data-dtp-today]');
		const clearButton = root.querySelector('[data-dtp-clear]');
		const closeButton = root.querySelector('[data-dtp-close]');

		if (!hidden || !trigger || !summary || !panel || !validation || !monthLabel || !yearMinusButton || !yearPlusButton || !yearInput || !dayMinusButton || !dayPlusButton || !dayInput || !weekdays || !daysGrid || !prevButton || !nextButton || !hourClock || !minuteClock || !hourMinusButton || !hourPlusButton || !hourInput || !minuteMinusButton || !minutePlusButton || !minuteInput || !amButton || !pmButton || !todayButton || !clearButton || !closeButton) {
			return;
		}

		const isRequired = root.dataset.required === 'true';
		const requiredMessage = root.dataset.requiredMessage || requiredDateMessage;
		const today = new Date();

		const state = {
			viewYear: today.getFullYear(),
			viewMonth: today.getMonth(),
			selectedYear: null,
			selectedMonth: null,
			selectedDay: null,
			selectedHour: null,
			selectedMinute: null,
			period: 'AM'
		};

		const hasDate = () => state.selectedYear !== null && state.selectedMonth !== null && state.selectedDay !== null;
		const hasTime = () => state.selectedHour !== null && state.selectedMinute !== null;
		const hasCompleteSelection = () => hasDate() && hasTime();

		const syncSelectedDayToMonth = () => {
			if (!hasDate()) {
				return;
			}

			const maxDay = new Date(state.selectedYear, state.selectedMonth, 0).getDate();
			if (state.selectedDay > maxDay) {
				state.selectedDay = maxDay;
			}
		};

		const getMaxDayForViewMonth = () => new Date(state.viewYear, state.viewMonth + 1, 0).getDate();

		const getSelectedDateTime = () => {
			if (!hasCompleteSelection()) {
				return null;
			}

			const hour12 = state.selectedHour === 12 ? 0 : state.selectedHour;
			const hour24 = state.period === 'PM' ? hour12 + 12 : hour12;
			return new Date(state.selectedYear, state.selectedMonth - 1, state.selectedDay, hour24, state.selectedMinute, 0, 0);
		};

		const setValidationState = (message, stateClass) => {
			validation.textContent = message || '';
			validation.classList.remove('field-validation-valid', 'field-validation-error');
			validation.classList.add(stateClass === 'valid' ? 'field-validation-valid' : 'field-validation-error');
			trigger.classList.toggle('field-missing', stateClass !== 'valid');
		};

		const renderSummary = () => {
			const dt = getSelectedDateTime();
			if (dt) {
				summary.textContent = summaryFormatter.format(dt);
				hidden.value = toISO(dt);
				return;
			}

			hidden.value = '';
			if (hasDate() && hasTime()) {
				summary.textContent = invalidMessage;
				return;
			}

			if (hasDate()) {
				const date = new Date(state.selectedYear, state.selectedMonth - 1, state.selectedDay);
				summary.textContent = `${dayFormatter.format(date)} • ${selectTimeText}`;
				return;
			}

			if (hasTime()) {
				summary.textContent = `${selectDateText} • ${state.selectedHour}:${String(state.selectedMinute).padStart(2, '0')}`;
				return;
			}

			summary.textContent = isHr ? 'Odaberite datum i vrijeme' : 'Select date and time';
		};

		const renderWeekdays = () => {
			weekdays.innerHTML = weekdayLabels.map((label) => `<div class="dtp-weekday">${label}</div>`).join('');
		};

		const renderCalendar = () => {
			const firstOfMonth = new Date(state.viewYear, state.viewMonth, 1);
			const daysInMonth = new Date(state.viewYear, state.viewMonth + 1, 0).getDate();
			const firstDayOffset = (firstOfMonth.getDay() - (isHr ? 1 : 0) + 7) % 7;
			const monthLabelDate = new Date(state.viewYear, state.viewMonth, 1);

			monthLabel.textContent = monthFormatter.format(monthLabelDate);

			let markup = '';
			for (let index = 0; index < firstDayOffset; index += 1) {
				markup += '<button type="button" class="dtp-day dtp-day--muted" tabindex="-1" aria-hidden="true" disabled></button>';
			}

			for (let day = 1; day <= daysInMonth; day += 1) {
				const isSelected = hasDate() && state.selectedYear === state.viewYear && state.selectedMonth === state.viewMonth + 1 && state.selectedDay === day;
				const dayDate = new Date(state.viewYear, state.viewMonth, day);
				markup += `
					<button type="button"
						class="dtp-day ${isSelected ? 'dtp-day--selected' : ''}"
						data-dtp-day="${day}"
						aria-label="${dayFormatter.format(dayDate)}">
						${day}
					</button>`;
			}

			daysGrid.innerHTML = markup;
		};

		const polarPosition = (angleDeg, radiusPercent) => {
			const angleRad = (angleDeg * Math.PI) / 180;
			return {
				x: `${Math.cos(angleRad) * radiusPercent}%`,
				y: `${Math.sin(angleRad) * radiusPercent}%`
			};
		};

		const renderHours = () => {
			let markup = '';
			for (let hour = 1; hour <= 12; hour += 1) {
				const angle = (hour / 12) * 360 - 90;
				const isSelected = state.selectedHour === hour;
				const isMajor = hour % 3 === 0;
				const position = polarPosition(angle, 38);
				markup += `
					<button type="button"
						class="dtp-clock__item dtp-clock__item--hour ${isMajor ? 'dtp-clock__item--major' : ''} ${isSelected ? 'dtp-clock__item--selected' : ''}"
						style="--dtp-x:${position.x}; --dtp-y:${position.y};"
						data-dtp-hour="${hour}"
						aria-label="${hour} ${hoursLabel}">${hour}</button>`;
			}

			const hourHandAngle = state.selectedHour === null ? -90 : ((state.selectedHour % 12) / 12) * 360 - 90;
			hourClock.innerHTML = `
				<div class="dtp-clock__ring"></div>
				<div class="dtp-clock__hand dtp-clock__hand--hour" style="--dtp-hand-angle:${hourHandAngle}deg"></div>
				<div class="dtp-clock__center" data-dtp-hour-display>${state.selectedHour === null ? '--' : String(state.selectedHour).padStart(2, '0')}</div>
				${markup}`;
		};

		const renderMinutes = () => {
			let markup = '';
			for (let minute = 0; minute < 60; minute += 1) {
				const angle = (minute / 60) * 360 - 90;
				const isSelected = state.selectedMinute === minute;
				const isMajor = minute % 5 === 0;
				const label = isMajor ? String(minute).padStart(2, '0') : '';
				const position = polarPosition(angle, 43);
				markup += `
					<button type="button"
						class="dtp-clock__item dtp-clock__item--minute ${isMajor ? 'dtp-clock__item--major' : 'dtp-clock__item--minor'} ${isSelected ? 'dtp-clock__item--selected' : ''}"
						style="--dtp-x:${position.x}; --dtp-y:${position.y};"
						data-dtp-minute="${minute}"
						aria-label="${minute} ${minutesLabel}">${label}</button>`;
			}

			const minuteHandAngle = state.selectedMinute === null ? -90 : (state.selectedMinute / 60) * 360 - 90;
			minuteClock.innerHTML = `
				<div class="dtp-clock__ring"></div>
				<div class="dtp-clock__hand dtp-clock__hand--minute" style="--dtp-hand-angle:${minuteHandAngle}deg"></div>
				<div class="dtp-clock__center" data-dtp-minute-display>${state.selectedMinute === null ? '--' : String(state.selectedMinute).padStart(2, '0')}</div>
				${markup}`;
		};

		const renderPeriodButtons = () => {
			amButton.classList.toggle('dtp-period-toggle__btn--active', state.period === 'AM');
			pmButton.classList.toggle('dtp-period-toggle__btn--active', state.period === 'PM');
		};

		const renderAll = () => {
			renderWeekdays();
			renderCalendar();
			renderHours();
			renderMinutes();
			renderPeriodButtons();
			renderSummary();
			hourInput.value = state.selectedHour === null ? '' : String(state.selectedHour).padStart(2, '0');
			minuteInput.value = state.selectedMinute === null ? '' : String(state.selectedMinute).padStart(2, '0');
			yearInput.value = String(state.viewYear);
			dayInput.value = state.selectedDay === null ? '' : String(state.selectedDay).padStart(2, '0');
		};

		const openPanel = () => {
			panel.hidden = false;
			trigger.setAttribute('aria-expanded', 'true');
			renderAll();
		};

		const closePanel = (validateNow = true) => {
			panel.hidden = true;
			trigger.setAttribute('aria-expanded', 'false');
			if (validateNow) {
				validate();
			}
		};

		const setDate = (year, month, day) => {
			state.selectedYear = year;
			state.selectedMonth = month;
			state.selectedDay = day;
			state.viewYear = year;
			state.viewMonth = month - 1;
			renderAll();
		};

		const setDay = (day) => {
			if (!Number.isFinite(day)) {
				return;
			}

			const maxDay = getMaxDayForViewMonth();
			const normalized = ((((Math.round(day) - 1) % maxDay) + maxDay) % maxDay) + 1;
			state.selectedYear = state.viewYear;
			state.selectedMonth = state.viewMonth + 1;
			state.selectedDay = normalized;
			renderAll();
		};

		const setHour = (hour) => {
			if (!Number.isFinite(hour)) {
				return;
			}

			state.selectedHour = ((((Math.round(hour) - 1) % 12) + 12) % 12) + 1;
			if (state.period === null) {
				state.period = 'AM';
			}
			renderAll();
		};

		const setMinute = (minute) => {
			if (!Number.isFinite(minute)) {
				return;
			}

			state.selectedMinute = ((Math.round(minute) % 60) + 60) % 60;
			renderAll();
		};

		const setPeriod = (period) => {
			state.period = period;
			renderAll();
		};

		const setViewYear = (year) => {
			if (!Number.isFinite(year)) {
				return;
			}

			const normalized = Math.min(2100, Math.max(1900, Math.round(year)));
			state.viewYear = normalized;

			if (hasDate()) {
				state.selectedYear = normalized;
				syncSelectedDayToMonth();
			}

			renderAll();
		};

		const validate = () => {
			const complete = hasCompleteSelection();
			const anySelection = hasDate() || hasTime();

			if (!anySelection) {
				if (isRequired) {
					setValidationState(requiredMessage, 'invalid');
					return false;
				}

				setValidationState('', 'valid');
				return true;
			}

			if (!complete) {
				setValidationState(partialMessage, 'invalid');
				return false;
			}

			const dt = getSelectedDateTime();
			if (!dt || Number.isNaN(dt.getTime())) {
				setValidationState(invalidMessage, 'invalid');
				return false;
			}

			hidden.value = toISO(dt);
			setValidationState('', 'valid');
			return true;
		};

		const applyMinuteFromInput = () => {
			const raw = (minuteInput.value || '').trim();
			if (raw.length === 0) {
				state.selectedMinute = null;
				renderAll();
				return;
			}

			if (!/^\d{1,2}$/.test(raw)) {
				setValidationState(invalidMinuteMessage, 'invalid');
				return;
			}

			const value = Number(raw);
			if (!Number.isFinite(value) || value < 0 || value > 59) {
				setValidationState(invalidMinuteMessage, 'invalid');
				return;
			}

			setMinute(value);
			if (state.selectedHour !== null || hasDate()) {
				setValidationState('', 'valid');
			}
		};

		const applyHourFromInput = () => {
			const raw = (hourInput.value || '').trim();
			if (raw.length === 0) {
				state.selectedHour = null;
				renderAll();
				return;
			}

			if (!/^\d{1,2}$/.test(raw)) {
				setValidationState(invalidHourMessage, 'invalid');
				return;
			}

			const value = Number(raw);
			if (!Number.isFinite(value) || value < 1 || value > 12) {
				setValidationState(invalidHourMessage, 'invalid');
				return;
			}

			setHour(value);
			if (state.selectedMinute !== null || hasDate()) {
				setValidationState('', 'valid');
			}
		};

		const applyYearFromInput = () => {
			const raw = (yearInput.value || '').trim();
			if (raw.length === 0) {
				renderAll();
				return;
			}

			if (!/^\d{4}$/.test(raw)) {
				setValidationState(invalidYearMessage, 'invalid');
				return;
			}

			const value = Number(raw);
			if (!Number.isFinite(value) || value < 1900 || value > 2100) {
				setValidationState(invalidYearMessage, 'invalid');
				return;
			}

			setViewYear(value);
			setValidationState('', 'valid');
		};

		const applyDayFromInput = () => {
			const raw = (dayInput.value || '').trim();
			if (raw.length === 0) {
				state.selectedDay = null;
				renderAll();
				return;
			}

			if (!/^\d{1,2}$/.test(raw)) {
				setValidationState(invalidDayMessage, 'invalid');
				return;
			}

			const value = Number(raw);
			const maxDay = getMaxDayForViewMonth();
			if (!Number.isFinite(value) || value < 1 || value > maxDay) {
				setValidationState(invalidDayMessage, 'invalid');
				return;
			}

			state.selectedYear = state.viewYear;
			state.selectedMonth = state.viewMonth + 1;
			state.selectedDay = value;
			renderAll();
			setValidationState('', 'valid');
		};

		const existingISO = parseISO(hidden.value);
		if (existingISO) {
			state.selectedYear = existingISO.getFullYear();
			state.selectedMonth = existingISO.getMonth() + 1;
			state.selectedDay = existingISO.getDate();
			state.selectedHour = getHour12(existingISO.getHours());
			state.selectedMinute = existingISO.getMinutes();
			state.period = existingISO.getHours() >= 12 ? 'PM' : 'AM';
			state.viewYear = existingISO.getFullYear();
			state.viewMonth = existingISO.getMonth();
		}

		trigger.addEventListener('click', () => {
			if (panel.hidden) {
				openPanel();
			}
		});

		trigger.addEventListener('keydown', (event) => {
			if (event.key === 'ArrowDown' || event.key === 'Enter' || event.key === ' ') {
				event.preventDefault();
				openPanel();
			}
		});

		root.addEventListener('focusout', (event) => {
			if (!root.contains(event.relatedTarget)) {
				validate();
			}
		});

		panel.addEventListener('click', (event) => {
			const dayButton = event.target.closest('[data-dtp-day]');
			if (dayButton) {
				setDate(state.viewYear, state.viewMonth + 1, Number(dayButton.dataset.dtpDay));
				return;
			}

			const hourButton = event.target.closest('[data-dtp-hour]');
			if (hourButton) {
				setHour(Number(hourButton.dataset.dtpHour));
				return;
			}

			const minuteButton = event.target.closest('[data-dtp-minute]');
			if (minuteButton) {
				setMinute(Number(minuteButton.dataset.dtpMinute));
				return;
			}
		});

		prevButton.addEventListener('click', () => {
			const result = clampMonth(state.viewYear, state.viewMonth - 1);
			state.viewYear = result.year;
			state.viewMonth = result.month;
			renderAll();
		});

		nextButton.addEventListener('click', () => {
			const result = clampMonth(state.viewYear, state.viewMonth + 1);
			state.viewYear = result.year;
			state.viewMonth = result.month;
			renderAll();
		});

		yearMinusButton.addEventListener('click', () => setViewYear(state.viewYear - 1));
		yearPlusButton.addEventListener('click', () => setViewYear(state.viewYear + 1));
		yearInput.addEventListener('input', () => {
			yearInput.value = yearInput.value.replace(/\D+/g, '').slice(0, 4);
		});
		yearInput.addEventListener('blur', applyYearFromInput);
		yearInput.addEventListener('keydown', (event) => {
			if (event.key === 'Enter') {
				event.preventDefault();
				applyYearFromInput();
			}
		});

		dayMinusButton.addEventListener('click', () => setDay((state.selectedDay ?? 1) - 1));
		dayPlusButton.addEventListener('click', () => setDay((state.selectedDay ?? 1) + 1));
		dayInput.addEventListener('input', () => {
			dayInput.value = dayInput.value.replace(/\D+/g, '').slice(0, 2);
		});
		dayInput.addEventListener('blur', () => {
			applyDayFromInput();
			validate();
		});
		dayInput.addEventListener('keydown', (event) => {
			if (event.key === 'Enter') {
				event.preventDefault();
				applyDayFromInput();
			}
		});

		amButton.addEventListener('click', () => setPeriod('AM'));
		pmButton.addEventListener('click', () => setPeriod('PM'));

		todayButton.addEventListener('click', () => {
			const now = new Date();
			state.selectedYear = now.getFullYear();
			state.selectedMonth = now.getMonth() + 1;
			state.selectedDay = now.getDate();
			state.selectedHour = getHour12(now.getHours());
			state.selectedMinute = now.getMinutes();
			state.period = now.getHours() >= 12 ? 'PM' : 'AM';
			state.viewYear = now.getFullYear();
			state.viewMonth = now.getMonth();
			renderAll();
		});

		clearButton.addEventListener('click', () => {
			state.selectedYear = null;
			state.selectedMonth = null;
			state.selectedDay = null;
			state.selectedHour = null;
			state.selectedMinute = null;
			state.period = 'AM';
			hidden.value = '';
			renderAll();
			setValidationState('', 'valid');
		});

		hourMinusButton.addEventListener('click', () => setHour((state.selectedHour ?? 1) - 1));
		hourPlusButton.addEventListener('click', () => setHour((state.selectedHour ?? 12) + 1));
		hourInput.addEventListener('input', () => {
			hourInput.value = hourInput.value.replace(/\D+/g, '').slice(0, 2);
		});
		hourInput.addEventListener('blur', () => {
			applyHourFromInput();
			validate();
		});
		hourInput.addEventListener('keydown', (event) => {
			if (event.key === 'Enter') {
				event.preventDefault();
				applyHourFromInput();
			}
		});

		minuteMinusButton.addEventListener('click', () => setMinute((state.selectedMinute ?? 0) - 1));
		minutePlusButton.addEventListener('click', () => setMinute((state.selectedMinute ?? 0) + 1));
		minuteInput.addEventListener('input', () => {
			minuteInput.value = minuteInput.value.replace(/\D+/g, '').slice(0, 2);
		});
		minuteInput.addEventListener('blur', () => {
			applyMinuteFromInput();
			validate();
		});
		minuteInput.addEventListener('keydown', (event) => {
			if (event.key === 'Enter') {
				event.preventDefault();
				applyMinuteFromInput();
			}
		});

		closeButton.addEventListener('click', () => closePanel(true));

		const form = root.closest('form');
		if (form) {
			if (!form._dtpValidators) {
				form._dtpValidators = [];
			}

			form._dtpValidators.push(validate);
		}

		renderAll();
	});
};

const wireFormSubmit = () => {
	document.querySelectorAll('form').forEach((form) => {
		// Disable browser native validation popups — we handle all validation ourselves
		form.setAttribute('novalidate', '');

		if (form._submitBound) {
			return;
		}

		form._submitBound = true;

		if (!form._autocompleteValidators) {
			form._autocompleteValidators = [];
		}

		if (!form._dtpValidators) {
			form._dtpValidators = [];
		}

		form.addEventListener('submit', (event) => {
			// jQuery validation (messages + built-in rules)
			let jqValid = true;
			if (window.jQuery && window.jQuery(form).data('validator')) {
				jqValid = window.jQuery(form).valid();
			}

			// Always run native validation (red borders; also sets messages when jQuery is absent)
			let nativeValid = true;
			form.querySelectorAll('input, select, textarea').forEach((field) => {
				nativeValid = validateNativeField(field) && nativeValid;
			});

			// Paint red borders for any extra jQuery-detected errors
			markJQueryErrors(form);

			const relationValid = validatePlayersRelation(form, true);

			let autocompleteValid = true;
			form._autocompleteValidators.forEach((validator) => {
				autocompleteValid = validator() && autocompleteValid;
			});

			let dtpValid = true;
			form._dtpValidators.forEach((validator) => {
				dtpValid = validator() && dtpValid;
			});

			if (!jqValid || !nativeValid || !relationValid || !autocompleteValid || !dtpValid) {
				event.preventDefault();
			}
		});
	});
};

const applyServerSideValidationStyles = () => {
	document.querySelectorAll('.field-validation-error[data-valmsg-for]').forEach((span) => {
		if (!span.textContent.trim()) {
			return;
		}

		const fieldName = span.dataset.valmsgFor;
		const form = span.closest('form');
		if (!form) {
			return;
		}

		const field = form.querySelector(`[name="${fieldName}"]`);
		if (field && field.type !== 'hidden') {
			field.classList.add('field-missing');
			return;
		}

		const dtpRoot = form.querySelector(`[data-dtp-hidden][name="${fieldName}"]`)?.closest('[data-dtp]');
		if (dtpRoot) {
			const trigger = dtpRoot.querySelector('[data-dtp-trigger]');
			if (trigger) {
				trigger.classList.add('field-missing');
			}
		}
	});
};

document.addEventListener('DOMContentLoaded', () => {
	wireAjaxSearch();
	wireFieldFeedback();
	wireAutocompleteDropdowns();
	wireDateTimePickers();
	wireFormSubmit();
	applyServerSideValidationStyles();

	const navLinks = document.querySelectorAll('.site-nav-link');
	const path = window.location.pathname.toLowerCase();
	const pathSegments = path.split('/').filter(Boolean);
	const firstSegment = (pathSegments[0] || '').toLowerCase();
	const currentController = firstSegment || 'home';
	const currentAction = (pathSegments[1] || 'index').toLowerCase();

	navLinks.forEach((link) => {
		link.classList.remove('active');
		const controller = (link.dataset.controller || '').toLowerCase();
		const route = (link.dataset.route || '').toLowerCase();
		const action = (link.dataset.action || '').toLowerCase();
		const matchesHome = !firstSegment && controller === 'home' && !action;
		const matchesRoute = firstSegment && route && firstSegment === route;
		const matchesController = firstSegment && controller && firstSegment === controller;

		if (!matchesHome && !matchesRoute && !matchesController) {
			return;
		}

		if (action && action !== currentAction && !matchesRoute) {
			return;
		}

		link.classList.add('active');
	});

	window.requestAnimationFrame(() => {
		window.requestAnimationFrame(() => {
			document.body.classList.add('nav-ready');
		});
	});
});
