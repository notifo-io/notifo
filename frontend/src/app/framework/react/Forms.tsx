/*
 * Notifo.io
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved.
 */

import { isErrorVisible } from '@app/framework/utils';
import { useField, useFormikContext } from 'formik';
import * as React from 'react';
import { Badge, Button, Col, CustomInput, CustomInputProps, FormGroup, Input, Label, Row } from 'reactstrap';
import { InputProps } from 'reactstrap/lib/Input';
import { FormControlError } from './FormControlError';
import { Icon } from './Icon';
import { LanguageSelector } from './LanguageSelector';
import { PasswordInput } from './PasswordInput';
import { Toggle } from './Toggle';

type Option<T> = { value: T | string, label: string };

export interface FormProps {
    // The input label
    label?: string;

    // The input hint
    hints?: string;

    // The name of the control.
    name: string;

    // The optional class name.
    className?: string;

    // True if disabled.
    disabled?: boolean;
}

export interface ArrayFormProps<T> extends FormProps {
    allowedValues: ReadonlyArray<T>;
}

export interface LocalizedFormProps extends FormProps {
    // The available languages.
    languages: ReadonlyArray<string>;

    // The selected language.
    language?: string;

    // Triggered when the language is selected.
    onLanguageSelect: (language: string) => void;
}

export interface BooleanFormProps extends FormProps {
    indeterminate?: boolean;
}

export interface OptionsFormProps<T> extends FormProps {
    options: Option<T>[];
}

const FormError = (props: { name: string }) => {
    const { name } = props;

    const { submitCount } = useFormikContext();
    const [, meta] = useField(name);

    return (
        <FormControlError error={meta.error} touched={meta.touched} submitCount={submitCount} />
    );
};

export module Forms {
    export const Error = FormError;

    export const LocalizedText = (props: LocalizedFormProps) => {
        const { className, label, language, languages, name, onLanguageSelect, ...other } = props;

        const fieldName = `${name}.${language}`;

        return (
            <FormGroup className={className}>
                <Row>
                    <Col>
                        <Label htmlFor={name}>{label}</Label>
                    </Col>
                    <Col xs='auto'>
                        <LanguageSelector
                            languages={languages}
                            language={language}
                            onSelect={onLanguageSelect} />
                    </Col>
                </Row>

                <FormError name={name} />

                <InputText {...other} name={fieldName} />
            </FormGroup>
        );
    };

    export const LocalizedTextArea = (props: LocalizedFormProps) => {
        const { className, label, language, languages, name, onLanguageSelect, ...other } = props;

        const fieldName = `${name}.${language}`;

        return (
            <FormGroup className={className}>
                <Row>
                    <Col>
                        <Label htmlFor={name}>{label}</Label>
                    </Col>
                    <Col xs='auto'>
                        <LanguageSelector
                            languages={languages}
                            language={language}
                            onSelect={onLanguageSelect} />
                    </Col>
                </Row>

                <FormError name={name} />

                <InputTextArea {...other} name={fieldName} />
            </FormGroup>
        );
    };

    export const Boolean = (props: BooleanFormProps) => {
        const { className } = props;

        return (
            <FormGroup className={className}>
                <InputToggle {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridBoolean = (props: BooleanFormProps) => {
        const { className } = props;

        return (
            <FormGroup className={className} row>
                <Col sm={4} htmlFor={props.name}></Col>

                <Col sm={8}>
                    <FormGroup>
                        <InputToggle {...props} />

                    <FormHint hints={props.hints} />
                    </FormGroup>
                </Col>
            </FormGroup>
        );
    };

    export const TextArea = (props: FormProps & InputProps) => {
        const { className, label } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={props.name}>{label}</Label>
                }

                <InputTextArea {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridTextArea = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    <InputTextArea {...props} />

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };

    export const Array = (props: ArrayFormProps<any>) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputArray {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const Text = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputText {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridText = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    <InputText {...props} />

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };

    export const Number = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputNumber {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridNumber = (props: FormProps & { units?: string } & InputProps) => {
        const { className, label, name, units } = props;

        return (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    <Row>
                        <Col xs={6}>
                            <InputNumber {...props} />
                        </Col>
                        <Label xs={6} htmlFor={name}>{units}</Label>
                    </Row>

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };

    export const Email = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputEmail {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridEmail = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    <InputEmail {...props} />

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };

    export const Password = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputPassword {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridPassword = (props: FormProps & InputProps) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className} row>
                {label ? (
                    <Label sm={4} htmlFor={name}>{label}</Label>
                ) : (
                    <Col sm={4} />
                )}

                <Col sm={8}>
                    <InputPassword {...props} />

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };

    export const Select = (props: OptionsFormProps<string | number> & Partial<CustomInputProps>) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className}>
                {label &&
                    <Label htmlFor={name}>{label}</Label>
                }

                <InputSelect {...props} />

                <FormHint hints={props.hints} />
            </FormGroup>
        );
    };

    export const GridSelect = (props: OptionsFormProps<string | number> & Partial<CustomInputProps>) => {
        const { className, label, name } = props;

        return (
            <FormGroup className={className} row>
                <Label sm={4} htmlFor={name}>{label}</Label>

                <Col sm={8}>
                    <InputSelect {...props} />

                    <FormHint hints={props.hints} />
                </Col>
            </FormGroup>
        );
    };
}

const InputNumber = ({ name }: FormProps) => {
    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <Input type='number' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
            />
        </>
    );
};

const InputText = (props: FormProps & InputProps) => {
    const { name, ...other } = props;

    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <Input type='text' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                {...other}
            />
        </>
    );
};

const InputTextArea = (props: FormProps & InputProps) => {
    const { name, ...other } = props;

    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <Input type='textarea' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                {...other}
            />
        </>
    );
};

const InputEmail = (props: FormProps & InputProps) => {
    const { name, ...other } = props;

    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <Input type='email' name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                {...other}
            />
        </>
    );
};

const InputPassword = (props: FormProps & InputProps) => {
    const { name, ...other } = props;

    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <PasswordInput name={name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                {...other}
            />
        </>
    );
};

const InputSelect = (props: OptionsFormProps<string | number> & Partial<CustomInputProps>) => {
    const { name, options, ...other } = props;

    const { submitCount } = useFormikContext();
    const [field, meta] = useField(name);

    return (
        <>
            <FormError name={name} />

            <CustomInput type='select' name={field.name} id={field.name} value={field.value || ''} invalid={isErrorVisible(meta.error, meta.touched, submitCount)}
                onChange={field.onChange}
                onBlur={field.onBlur}
                {...other}
            >
                {options.map(option =>
                    <option key={option.value} value={option.value}>{option.label}</option>,
                )}
            </CustomInput>
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

const InputArray = (props: ArrayFormProps<any>) => {
    const { allowedValues, name } = props;

    const [field, , helpers] = useField<any[]>(name);
    const fieldValue = field.value || [];

    const [newValue, setNewValue] = React.useState<any>(undefined);

    const newValues = React.useMemo(() => {
        return allowedValues.filter(x => fieldValue.indexOf(x) < 0);
    }, [fieldValue, allowedValues]);

    React.useEffect(() => {
        setNewValue(newValues[0]);
    }, [newValues]);

    const doRemove = React.useCallback((value: any) => {
        helpers.setValue(fieldValue.filter(x => x !== value));
    }, [fieldValue]);

    const doAdd = React.useCallback(() => {
        helpers.setValue([...fieldValue, newValue]);
    }, [fieldValue, newValue]);

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

const FormHint = ({ hints }: { hints?: string }) => {
    if (!hints) {
        return null;
    }

    return (
        <div>
            <small className='text-muted'>
                {hints}
            </small>
        </div>
    );
};
