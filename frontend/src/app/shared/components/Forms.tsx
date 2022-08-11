/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import classNames from 'classnames';
import { FieldHelperProps, FieldInputProps, FieldMetaProps, FormikContextType, useField, useFormikContext } from 'formik';
import * as React from 'react';
import { Badge, Button, Col, CustomInput, FormGroup, Input, InputGroup, InputGroupAddon, InputGroupText, Label } from 'reactstrap';
import { FormControlError, Icon, LanguageSelector, PasswordInput, Toggle } from '@app/framework';
import { isErrorVisible, Types } from '@app/framework/utils';
import { Picker, PickerOptions } from './Picker';

export type FormEditorOption<T> = { value?: T; label: string };

export interface FormEditorProps {
    // The label.
    label?: string;

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

    // True if disabled.
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
    // True if 3 states are allowed.
    indeterminate?: boolean;

    // True to provide the value as string.
    asString?: boolean;
}

export interface OptionsFormProps<T> extends FormEditorProps {
    // The allowed selected values.
    options: FormEditorOption<T>[];
}

export interface FormRowProps extends FormEditorProps {
    // The children.
    children: React.ReactNode;
}

export interface LocalizedFormProps extends FormEditorProps {
    // The available languages.
    languages: ReadonlyArray<string>;

    // The selected language.
    language?: string;

    // Triggered when the language is selected.
    onLanguageSelect: (language: string) => void;
}

export function useFieldNew<T = any>(name: string): [FieldInputProps<T>, FieldMetaProps<T>, FieldHelperProps<T>] {
    const [field, meta] = useField<T>(name);
    const { setFieldTouched, setFieldValue, setFieldError } = useFormikContext();

    const helpers = React.useMemo(() => ({
        setValue: (value: T, shouldValidate?: boolean) => {
            setFieldValue(field.name, value, shouldValidate);
        },
        setTouched: (isTouched?: boolean, shouldValidate?: boolean) => {
            setFieldTouched(field.name, isTouched, shouldValidate);
        },
        setError: (message?: string) => {
            setFieldError(field.name, message);
        },
    }), [setFieldTouched, setFieldValue, setFieldError, field.name]);

    return [field, meta, helpers];
}

export module Forms {
    export type Option<T = any> = { value?: T | string; label: string };

    export const Error = (props: { name: string }) => {
        const { name } = props;

        const { submitCount } = useFormikContext();
        const [, meta] = useFieldNew(name);

        return (
            <FormControlError error={meta.error} touched={meta.touched} submitCount={submitCount} />
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

    export const Text = ({ placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputText name={other.name} placeholder={placeholder} />
            </Forms.Row>
        );
    };

    export const Url = ({ placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputUrl name={other.name} placeholder={placeholder} />
            </Forms.Row>
        );
    };

    export const Textarea = ({ placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputTextarea name={other.name} placeholder={placeholder} />
            </Forms.Row>
        );
    };

    export const Number = ({ max, min, placeholder, step, unit, ...other }: FormEditorProps & { unit?: string; min?: number; max?: number; step?: number }) => {
        return (
            <Forms.Row {...other}>
                <InputGroup>
                    <InputNumber name={other.name} placeholder={placeholder} max={max} min={min} step={step} />

                    {unit &&
                        <InputGroupAddon addonType='prepend'>
                            <InputGroupText>{unit}</InputGroupText>
                        </InputGroupAddon>
                    }
                </InputGroup>
            </Forms.Row>
        );
    };

    export const Email = ({ placeholder, ...other }: FormEditorProps) => {
        return (
            <Forms.Row {...other}>
                <InputEmail name={other.name} placeholder={placeholder} />
            </Forms.Row>
        );
    };

    export const Password = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputPassword name={props.name} placeholder={props.placeholder} />
            </Forms.Row>
        );
    };

    export const Select = ({ options, placeholder, ...other }: FormEditorProps & { options: Option<string | number>[] }) => {
        return (
            <Forms.Row {...other}>
                <InputSelect name={other.name} placeholder={placeholder} options={options} />
            </Forms.Row>
        );
    };

    export const Checkboxes = ({ options, ...other }: FormEditorProps & { options: Option<string>[] }) => {
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

const InputNumber = ({ name, max, min, placeholder, step }: FormEditorProps & { min?: number; max?: number; step?: number }) => {
    const { submitCount } = useFormikContext();
    const [field, meta, helper] = useFieldNew(name);

    const doChange = React.useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
        helper.setValue(parseInt(event.target.value, 10));
    }, [helper]);

    return (
        <>
            <Input type='number' name={name} id={field.name} value={field.value} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                max={max}
                min={min}
                step={step}
                onChange={doChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </>
    );
};

const InputText = ({ name, picker, placeholder }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta, helper] = useFieldNew(name);

    const doAddPick = React.useCallback((value: string) => {
        helper.setValue((field.value || '') + value);
    }, [field.value, helper]);

    return (
        <div className='input-container'>
            {picker &&
                <Picker {...picker} onPick={doAddPick} value={field.value} />
            }

            <Input type='text' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </div>
    );
};

const InputTextarea = ({ name, picker, placeholder }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta, helper] = useFieldNew(name);

    const doAddPick = React.useCallback((value: string) => {
        helper.setValue((field.value || '') + value);
    }, [field.value, helper]);

    return (
        <div className='input-container textarea'>
            {picker &&
                <Picker {...picker} onPick={doAddPick} value={field.value} />
            }

            <Input type='textarea' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </div>
    );
};

const InputUrl = ({ name, placeholder }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useFieldNew(name);

    return (
        <>
            <Input type='url' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </>
    );
};

const InputEmail = ({ name, placeholder }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useFieldNew(name);

    return (
        <>
            <Input type='email' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </>
    );
};

const InputPassword = ({ name, placeholder }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useFieldNew(name);

    return (
        <>
            <PasswordInput name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                placeholder={placeholder}
            />
        </>
    );
};

const InputToggle = (props: BooleanFormProps) => {
    const { name } = props;
    const [field, , helpers] = useFieldNew(name);

    return (
        <>
            <Toggle {...props} value={field.value} onChange={helpers.setValue} />
        </>
    );
};

const InputSelect = ({ name, options }: FormEditorProps & { options: Forms.Option<string | number>[] }) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField<string | number>(name);

    return (
        <>
            <CustomInput type='select' name={field.name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            >
                {Types.isUndefined(field.value) && !options.find(x => x.value === field.value) &&
                    <option></option>
                }

                {options.map((option, i) =>
                    <option key={i} value={option.value}>{option.label}</option>,
                )}
            </CustomInput>
        </>
    );
};

const InputCheckboxes = ({ name, options }: FormEditorProps & { options: Forms.Option<string>[] }) => {
    const form = useFormikContext();
    const [field] = useFieldNew(name);

    return (
        <div style={{ paddingTop: '.625rem' }}>
            {options.map(option =>
                <InputCheckboxOption key={option.value} field={field} form={form} option={option} />,
            )}
        </div>
    );
};

const EMPTY_ARRAY: any[] = [];

const InputCheckboxOption = (props: { field: FieldInputProps<string[]>; form: FormikContextType<unknown>; option: Forms.Option<string> }) => {
    const { field, form, option } = props;

    const valueArray: string[] = field.value || EMPTY_ARRAY;
    const valueExist = valueArray && valueArray.indexOf(option.value!) >= 0;

    const doChange = React.useCallback(() => {
        if (valueExist) {
            form.setFieldValue(field.name, valueArray.filter(x => x !== option.value));
        } else {
            form.setFieldValue(field.name, [...valueArray, option.value]);
        }
    }, [valueExist, form, field.name, valueArray, option.value]);

    return (
        <CustomInput type='checkbox' name={option.value} id={option.value || 'none'} checked={valueExist}
            onChange={doChange}
            onBlur={field.onBlur}
            label={option.label}
        />
    );
};

const InputArray = (props: ArrayFormProps<any>) => {
    const { allowedValues, name } = props;

    const [field, , helpers] = useField<any[]>(name);
    const fieldValue = field.value || EMPTY_ARRAY;

    const [newValue, setNewValue] = React.useState<any>(undefined);

    const newValues = React.useMemo(() => {
        return allowedValues.filter(x => fieldValue.indexOf(x) < 0);
    }, [fieldValue, allowedValues]);

    React.useEffect(() => {
        setNewValue(newValues[0]);
    }, [newValues]);

    const doRemove = React.useCallback((value: any) => {
        helpers.setValue(fieldValue.filter(x => x !== value));
    }, [fieldValue, helpers]);

    const doAdd = React.useCallback(() => {
        helpers.setValue([...fieldValue, newValue]);
    }, [fieldValue, helpers, newValue]);

    const doSelectValue = React.useCallback((ev: React.ChangeEvent<HTMLInputElement>) => {
        setNewValue(ev.target.value);
    }, []);

    return (
        <div>
            <div>
                {fieldValue.map(v => (
                    <Badge key={v} color='secondary' className='mr-2 mb-2 badge-lg'>
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
            <Forms.Error name={name} />

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
            <Forms.Error name={name} />

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
