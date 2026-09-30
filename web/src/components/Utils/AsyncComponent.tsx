import type { AxiosError } from "axios";
import { type ReactNode } from "react";
import LogoLoader from "./LogoLoader";
import ErrorHandler from "./Error/ErrorHandler";

interface AsyncComponentProps<T> {
  query: {
    isLoading: boolean;
    error: Error | AxiosError | null;
    data: T | undefined;
  };
  render404?: ReactNode;
  render: (data: T) => ReactNode;
  renderLoading?: ReactNode;
}
export default function AsyncComponent<T>(props: AsyncComponentProps<T>) {
  const { query, render, render404 } = props;

  if (query.isLoading) return props.renderLoading || <LogoLoader />;
  if (query.error)
    return <ErrorHandler custom404Render={render404} error={query.error} />;
  if (query.data === undefined) return null;

  return render(query.data);
}
