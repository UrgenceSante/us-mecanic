interface DateDisplayerProps {
  value: string | Date | null | undefined;
  options?: Intl.DateTimeFormatOptions;
  locale?: string;
}
export default function DateDisplayer(props: DateDisplayerProps) {
  const defaultOptions: Intl.DateTimeFormatOptions = {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  };

  const defaultLocale = "fr-FR";

  if (!props.value) return <>-</>;

  const date =
    typeof props.value === "string"
      ? new Date(props.value).toLocaleString(
          props.locale || defaultLocale,
          props.options || defaultOptions
        )
      : props.value;

  return <>{date}</>;
}
