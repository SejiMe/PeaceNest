export type ApiProblemDetails = {
  status?: number;
  title?: string;
  detail?: string;
  errorCode?: string;
  errors?: Record<string, string[]>;
  traceId?: string;
};

export class ApiError extends Error {
  readonly status: number;
  readonly problem?: ApiProblemDetails;

  constructor(status: number, message: string, problem?: ApiProblemDetails) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }
}
