import { Pipe, PipeTransform } from '@angular/core';

/**
 * Remove caracteres de controle Unicode perigosos do texto:
 * - RTLO (U+202E): inverte a direção do texto — usado para mascarar extensões de arquivo
 * - ZWNJ, ZWJ, outros bidirectional markers
 * - Caracteres de controle C0 (exceto newline e tab)
 */
@Pipe({
  name: 'safeText',
  standalone: true,
})
export class SafeTextPipe implements PipeTransform {
  // Regex para caracteres de controle perigosos
  private static readonly DANGEROUS_CHARS = /[\u202E\u200B\u200C\u200D\u2066\u2067\u2068\u2069\u202A-\u202D\u2028\u2029\u0000-\u0008\u000B\u000C\u000E-\u001F\u007F]/g;

  transform(value: string | null | undefined): string {
    if (!value) return '';
    return value.replace(SafeTextPipe.DANGEROUS_CHARS, '');
  }
}
