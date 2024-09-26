/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import * as React from 'react';
import { ControllerFieldState, FormState, get, useController, useFormState } from 'react-hook-form';
import { Badge, Button, Col, CustomInput, FormGroup, Input, InputGroup, InputGroupAddon, InputGroupText, Label } from 'reactstrap';
import { InputType } from 'reactstrap/es/Input';
import { CodeEditor, CodeEditorProps, FormControlError, Icon, LanguageSelector, PasswordInput, Toggle, useEventCallback } from '@app/framework';
import { Types } from '@app/framework/utils';
import { Picker, PickerOptions } from './Picker';

export type FormEditorOption<T> = { value?: T | undefined; label: string };

export interface FormEditorProps {
    // The label.
    label?: string;

    // Indicates that the input should be focused automatically.
    autoFocus?: boolean;

    // The optional class name.
    className?: string;

    // The optional placeholder.
    placeholder?: string;

    // The hints.
    hints?: string;

    // The form name.
    name: string;

    // The layout.
    vertical?: boolean;

    // True, if disabled.
    disabled?: boolean;

    // True to hide the error.
    hideError?: boolean;

    // The picker props.
    picker?: PickerOptions;
}

export interface ArrayFormProps<T> extends FormEditorProps {
    // The allowed values.
    allowedValues: ReadonlyArray<T>;
}

export interface BooleanFormProps extends FormEditorProps {
    // True, if 3 states are allowed.
    indeterminate?: boolean;

    // True to provide the value as string.
    asString?: boolean;
}

export interface CodeFormProps extends FormEditorProps {
    // The initial options.
    initialOptions?: CodeEditorProps['initialOptions'];
}

export interface OptionsFormProps<T> extends FormEditorProps {
    // The allowed selected values.
    options: FormEditorOption<T>[];
}

export interface FormRowProps extends FormEditorProps, React.PropsWithChildren<any> {}

export interface LocalizedFormProps extends FormEditorProps {
    // The available languages.
    languages: ReadonlyArray<string>;

    // The selected language.
    language?: string;

    // Triggered when the language is selected.
    onLanguageSelect: (language: string) => void;
}

export module Forms {
    export const Error = ({ name }: { name: string }) => {
        const { errors, submitCount, touchedFields } = useFormState({ name });
        const error = get(errors, name);

        return (
            <FormControlError error={error?.message} submitCount={submitCount} touched={get(touchedFields, name)} />
        );
    };

    export const Row = (props: FormRowProps) => {
        const { children, className, hideError, hints, name, label, vertical } = props;

        return vertical ? (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                {!hideError &&
                    <Forms.Error name={name} />
                }

                {children}

                <FormDescription hints={hints} />
            </FormGroup>
        ) : (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    {!hideError &&
                        <Forms.Error name={name} />
                    }

                    {children}

                    <FormDescription hints={hints} />
                </Col>
            </FormGroup>
        );
    };

    export const LocalizedText = ({ className, ...other }: LocalizedFormProps) => {
        return (
            <Forms.Row className={classNames(className, 'localized-value')} {...other} hideError>
                <InputLocalizedText {...other} />
            </Forms.Row>
        );
    };

    export const LocalizedTextArea = ({ className, ...other }: LocalizedFormProps) => {
        return (
            <Forms.Row className={classNames(className, 'localized-value')} {...other} hideError>
                <InputLocalizedTextArea {...other} />
            </Forms.Row>
        );
    };

    export const LocalizedCode = ({ className, ...other }: LocalizedFormProps & CodeFormProps) => {
        return (
            <Forms.Row className={classNames(className, 'localized-value')} {...other} hideError>
                <InputLocalizedCode {...other} />
            </Forms.Row>
        );
    };

    export const Boolean = ({ label, ...other }: BooleanFormProps) => {
        return (
            <Forms.Row {...other}>
                <InputToggle name={other.name} label={label!} />
            </Forms.Row>
        );
    };

    export const Array = (props: ArrayFormProps<any>) => {
        return (
            <Forms.Row {...props}>
                <InputArray {...props} />
            </Forms.Row>
        );
    };

    export const Text = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputText name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Phone = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputSpecial type='tel' name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Url = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputSpecial type='url' name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Email = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputSpecial type='email' name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Time = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputSpecial type='time' name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Date = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputSpecial type='date' name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Textarea = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputTextarea name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Number = ({ autoFocus, max, min, placeholder, step, unit, ...other }: FormEditorProps & { unit?: string; min?: number; max?: number; step?: number }) => {
        return (
            <Forms.Row {...other}>
                <InputGroup>
                    <InputNumber name={other.name} placeholder={placeholder} max={max} min={min} step={step} autoFocus={autoFocus} />

                    {unit &&
                        <InputGroupAddon addonType='prepend'>
                            <InputGroupText>{unit}</InputGroupText>
                        </InputGroupAddon>
                    }
                </InputGroup>
            </Forms.Row>
        );
    };

    export const Password = ({ autoFocus, placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputPassword name={other.name} placeholder={placeholder} autoFocus={autoFocus} />
            </Forms.Row>
        );
    };

    export const Select = ({ options, placeholder, ...other }: FormEditorProps & { options: ReadonlyArray<FormEditorOption<string | number | undefined>> }) => {
        return (
            <Forms.Row {...other}>
                <InputSelect name={other.name} placeholder={placeholder} options={options} />
            </Forms.Row>
        );
    };

    export const Checkboxes = ({ options, ...other }: FormEditorProps & { options: ReadonlyArray<FormEditorOption<string>> }) => {
        if (!options || options.length === 0) {
            return null;
        }

        return (
            <Forms.Row {...other}>
                <InputCheckboxes name={other.name} options={options} />
            </Forms.Row>
        );
    };
}

const FormDescription = ({ hints }: { hints?: string }) => {
    if (!hints) {
        return null;
    }

    return (
        <div className='text-muted'>
            <small>{hints}</small>
        </div>
    );
};

const InputText = ({ autoFocus, name, picker, placeholder }: FormEditorProps) => {
    const { field, fieldState, formState } = useController({ name });

    const doAddPick = useEventCallback((pick: string) => {
        field.onChange((field.value || '') + pick, SET_OPTIONS);
    });

    return (
        <div className='input-container'>
            {picker &&
                <Picker {...picker} onPick={doAddPick} value={field.value} />
            }

            <Input type='text' id={name} {...field} invalid={isInvalid(fieldState, formState)} autoFocus={autoFocus}
                placeholder={placeholder}
            />
        </div>
    );
};

const InputTextarea = ({ autoFocus, name, picker, placeholder }: FormEditorProps) => {
    const { field, fieldState, formState } = useController({ name });

    const doAddPick = useEventCallback((pick: string) => {
        field.onChange((field.value || '') + pick, SET_OPTIONS);
    });

    return (
        <div className='input-container textarea'>
            {picker &&
                <Picker {...picker} onPick={doAddPick} value={field.value} />
            }

            <Input type='textarea' id={name} {...field} invalid={isInvalid(fieldState, formState)} autoFocus={autoFocus}
                placeholder={placeholder}
            />
        </div>
    );
};

const InputNumber = ({ autoFocus, name, max, min, placeholder, step }: FormEditorProps & { min?: number; max?: number; step?: number }) => {
    const { field, fieldState, formState } = useController({ name });

    return (
        <>
            <Input type='number' id={name} {...field} invalid={isInvalid(fieldState, formState)} autoFocus={autoFocus}
                max={max} min={min} step={step} placeholder={placeholder}
            />
        </>
    );
};

const InputSpecial = ({ autoFocus, name, placeholder, type }: FormEditorProps & { type: InputType }) => {
    const { field, fieldState, formState } = useController({ name });

    return (
        <>
            <Input type={type} id={name} {...field} invalid={isInvalid(fieldState, formState)} autoFocus={autoFocus}
                placeholder={placeholder}
            />
        </>
    );
};
const InputPassword = ({ name, placeholder }: FormEditorProps) => {
    const { field, fieldState, formState } = useController({ name });

    return (
        <>
            <PasswordInput id={name} {...field} invalid={isInvalid(fieldState, formState)} autoFocus={autoFocus}
                placeholder={placeholder}
            />
        </>
    );
};

const InputToggle = ({ name, ...other }: BooleanFormProps) => {
    const { field } = useController({ name });

    return (
        <>
            <Toggle {...other} value={field.value} onChange={field.onChange} />
        </>
    );
};

const InputCode = ({ name, ...other }: CodeFormProps) => {
    const { field } = useController({ name });

    return (
        <>
            <CodeEditor {...other} value={field.value} onBlur={field.onBlur} onChange={field.onChange} />
        </>
    );
};

const InputSelect = ({ name, options }: FormEditorProps & { options: ReadonlyArray<FormEditorOption<string | number | undefined>> }) => {
    const { field, fieldState, formState } = useController({ name });

    const doChange = useEventCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.value;

        if (value === SET_UNDEFINED) {
            field.onChange(undefined);
        } else {
            field.onChange(event);
        }
    });

    return (
        <>
            <CustomInput type='select' id={name} {...field} onChange={doChange} invalid={isInvalid(fieldState, formState)}>
                {Types.isUndefined(field.value) && !options.find(x => x.value === field.value) &&
                    <option></option>
                }

                {options.map((option, i) =>
                    <option key={i} value={Types.isUndefined(option.value) ? SET_UNDEFINED : option.value}>{option.label}</option>,
                )}
            </CustomInput>
        </>
    );
};

const InputCheckboxes = ({ name, options }: FormEditorProps & { options: ReadonlyArray<FormEditorOption<string>> }) => {
    return (
        <div style={{ paddingTop: '.625rem' }}>
            {options.map(option =>
                <InputCheckboxOption key={option.value} name={name} option={option} />,
            )}
        </div>
    );
};

const EMPTY_ARRAY: any[] = [];

const SET_OPTIONS = { shouldTouch: true, shouldDirty: true, shouldValidate: true };
const SET_UNDEFINED = '__UNDEFINED';

const InputCheckboxOption = ({ name, option }: { name: string; option: FormEditorOption<string> }) => {
    const { field } = useController({ name });
    const fieldArray = field.value as string[] || EMPTY_ARRAY;
    const fieldChecked = fieldArray && fieldArray.indexOf(option.value!) >= 0;

    const doChange = useEventCallback(() => {
        if (fieldChecked) {
            field.onChange(fieldArray.filter(x => x !== option.value), SET_OPTIONS);
        } else {
            field.onChange([...fieldArray, option.value], SET_OPTIONS);
        }
    });

    return (
        <CustomInput type='checkbox' name={option.value} id={option.value || 'none'} checked={fieldChecked} onChange={doChange}
            label={option.label}
        />
    );
};

const InputArray = ({ allowedValues, name }: ArrayFormProps<any>) => {
    const { field } = useController({ name });
    const fieldValue = field.value as any[];
    const fieldArray = fieldValue || EMPTY_ARRAY;
    const [newValue, setNewValue] = React.useState<any>(undefined);

    const newValues = React.useMemo(() => {
        return allowedValues.filter(x => fieldArray.indexOf(x) < 0);
    }, [fieldArray, allowedValues]);

    React.useEffect(() => {
        setNewValue(newValues[0]);
    }, [newValues]);

    const doRemove = useEventCallback((value: any) => {
        field.onChange(fieldArray.filter(x => x !== value), SET_OPTIONS);
    });

    const doAdd = useEventCallback(() => {
        field.onChange([...fieldArray, newValue], SET_OPTIONS);
    });

    const doSelectValue = useEventCallback((ev: React.ChangeEvent<HTMLInputElement>) => {
        setNewValue(ev.target.value);
    });

    return (
        <div>
            <div>
                {fieldArray.map(v => (
                    <Badge key={v} color='secondary' className='mr-2 mb-2 badge-lg badge-option'>
                        {v}

                        <span onClick={() => doRemove(v)}>
                            <Icon type='clear' />
                        </span>
                    </Badge>
                ))}
            </div>

            {newValues.length > 0 &&
                <div>
                    <CustomInput type='select' id={name} value={newValue} onChange={doSelectValue} style={{ width: '100px' }}>
                        {newValues.map(v => (
                            <option key={v} value={v}>{v}</option>
                        ))}
                    </CustomInput>

                    <Button type='button' color='success' className='ml-2' onClick={doAdd}>
                        <Icon type='add' />
                    </Button>
                </div>
            }
        </div>
    );
};

const InputLocalizedText = (props: LocalizedFormProps) => {
    const { className, label, language, languages, name, onLanguageSelect, ...other } = props;
    const fieldName = `${name}.${language}`;

    return (
        <div className='localized-input'>
            <Forms.Error name={`${name}.root`} />

            <div className='localized-languages'>
                <LanguageSelector
                    languages={languages}
                    language={language}
                    onSelect={onLanguageSelect} />
            </div>

            <InputText {...other} name={fieldName} />
        </div>
    );
};

const InputLocalizedTextArea = (props: LocalizedFormProps) => {
    const { className, label, language, languages, name, onLanguageSelect, ...other } = props;
    const fieldName = `${name}.${language}`;

    return (
        <div className='localized-input'>
            <Forms.Error name={`${name}.root`} />

            <div className='localized-languages'>
                <LanguageSelector
                    languages={languages}
                    language={language}
                    onSelect={onLanguageSelect} />
            </div>

            <InputTextarea {...other} name={fieldName} />
        </div>
    );
};

const InputLocalizedCode = (props: LocalizedFormProps & CodeFormProps) => {
    const { className, label, language, languages, name, onLanguageSelect, ...other } = props;
    const fieldName = `${name}.${language}`;

    return (
        <div className='localized-input'>
            <Forms.Error name={`${name}.root`} />

            <div className='localized-languages'>
                <LanguageSelector
                    languages={languages}
                    language={language}
                    onSelect={onLanguageSelect} />
            </div>

            <InputCode {...other} name={fieldName} />
        </div>
    );
};

export function isInvalid(fieldState: ControllerFieldState, formState: FormState<any>) {
    return !!fieldState.error && (fieldState.isTouched || formState.submitCount > 0);
}