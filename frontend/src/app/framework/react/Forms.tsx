/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { isErrorVisible, Types } from '@app/framework/utils';
import { FieldInputProps, FormikContextType, useField, useFormikContext } from 'formik';
import * as React from 'react';
import { Badge, Button, Col, CustomInput, FormGroup, Input, InputGroup, InputGroupAddon, InputGroupText, Label } from 'reactstrap';
import { FormControlError } from './FormControlError';
import { Icon } from './Icon';
import { LanguageSelector } from './LanguageSelector';
import { PasswordInput } from './PasswordInput';
import { Toggle } from './Toggle';

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

export module Forms {
    export type Option<T = any> = { value?: T | string; label: string };

    export const Error = (props: { name: string }) => {
        const { name } = props;

        const { submitCount } = useFormikContext();
        const [, meta] = useField(name);

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
        const clazz = `localized-value ${className}`;

        return (
            <Forms.Row className={clazz} {...other} hideError>
                <InputLocalizedText {...other} />
            </Forms.Row>
        );
    };

    export const LocalizedTextArea = ({ className, ...other }: LocalizedFormProps) => {
        const clazz = `localized-value ${className}`;

        return (
            <Forms.Row className={clazz} {...other} hideError>
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

    export const Text = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputText name={props.name} />
            </Forms.Row>
        );
    };

    export const Url = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputUrl name={props.name} />
            </Forms.Row>
        );
    };

    export const Textarea = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputTextarea name={props.name} />
            </Forms.Row>
        );
    };

    export const Number = ({ max, min, step, unit, ...other }: FormEditorProps & { unit?: string; min?: number; max?: number; step?: number }) => {
        return (
            <Forms.Row {...other}>
                <InputGroup>
                    <InputNumber name={other.name} max={max} min={min} step={step} />

                    {unit &&
                        <InputGroupAddon addonType='prepend'>
                            <InputGroupText>{unit}</InputGroupText>
                        </InputGroupAddon>
                    }
                </InputGroup>
            </Forms.Row>
        );
    };

    export const Email = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputEmail name={props.name} />
            </Forms.Row>
        );
    };

    export const Password = (props: FormEditorProps) => {
        return (
            <Forms.Row {...props}>
                <InputPassword name={props.name} />
            </Forms.Row>
        );
    };

    export const Select = ({ options, ...other }: FormEditorProps & { options: Option<string | number>[] }) => {
        return (
            <Forms.Row {...other}>
                <InputSelect name={other.name} options={options} />
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

const InputNumber = ({ name, max, min, step }: FormEditorProps & { min?: number; max?: number; step?: number }) => {
    const { submitCount } = useFormikContext();
    const [field, meta, helper] = useField(name);

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
            />
        </>
    );
};

const InputText = ({ name }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <Input type='text' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputUrl = ({ name }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <Input type='url' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputTextarea = ({ name }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <Input type='textarea' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputEmail = ({ name }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <Input type='email' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputPassword = ({ name }: FormEditorProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <PasswordInput name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputToggle = (props: BooleanFormProps) => {
    const { name } = props;

    const [field, , helpers] = useField(name);

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
    const [field] = useField(name);
    const form = useFormikContext();

    return (
        <>
            {options.map(option =>
                <InputCheckboxOption key={option.value} field={field} form={form} option={option} />,
            )}
        </>
    );
};

const EMPTY_ARRAY: any[] = [];

const InputCheckboxOption = (props: { field: FieldInputProps<string[]>; form: FormikContextType<unknown>; option: Forms.Option<string> }) => {
    const { field, form, option } = props;

    const value: string[] = field.value || EMPTY_ARRAY;
    const valueExist = value && value.indexOf(option.value!) >= 0;

    const doChange = React.useCallback(() => {
        if (valueExist) {
            form.setFieldValue(field.name, value.filter(x => x !== option.value));
        } else {
            form.setFieldValue(field.name, [...value, option.value]);
        }
    }, [valueExist, form, field.name, value, option.value]);

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

            <LanguageSelector
                languages={languages}
                language={language}
                onSelect={onLanguageSelect} />

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

            <LanguageSelector
                languages={languages}
                language={language}
                onSelect={onLanguageSelect} />

            <InputTextarea {...other} name={fieldName} />
        </div>
    );
};
