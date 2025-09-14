import { HttpInterceptorFn } from "@angular/common/http";

export const apiHttpInterceptorFn: HttpInterceptorFn = (req, next) => {
  const apiVersion = '1.0';
  const cloned = req.clone({
    setHeaders: {
      'x-api-version': apiVersion
    }
  });
  return next(cloned);
};
